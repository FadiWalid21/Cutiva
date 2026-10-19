using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CuttingOptimizer.Application.Abstractions.Pricing;
using CuttingOptimizer.Application.Features.Pricing;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Infrastructure.Pricing;

public sealed class PricingDisplayService(
    ApplicationDbContext db,
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    ILogger<PricingDisplayService> logger) : IPricingDisplayService
{
    public const string FromCurrency = "SAR";
    public const string ToCurrency = "EGP";
    public const decimal SeedFallbackRate = 13.0m;

    private static readonly TimeSpan FxMemoryTtl = TimeSpan.FromHours(1);
    private const string FxCacheKey = "fx:SAR:EGP";

    public async Task<PricingDisplayDto> GetDisplayAsync(
        string? clientIp,
        CancellationToken cancellationToken = default)
    {
        var countryCode = await ResolveCountryCodeAsync(clientIp, cancellationToken);
        var useEgp = string.Equals(countryCode, "EG", StringComparison.OrdinalIgnoreCase);

        decimal? rate = null;
        var rateSource = "none";
        if (useEgp)
        {
            (rate, rateSource) = await ResolveSarToEgpRateAsync(cancellationToken);
        }

        var currency = useEgp ? ToCurrency : FromCurrency;
        var plans = await db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceSar)
            .ToListAsync(cancellationToken);

        var displayPlans = plans
            .Select(p =>
            {
                var amount = useEgp && rate is > 0
                    ? decimal.Truncate(p.PriceSar * rate.Value)
                    : decimal.Truncate(p.PriceSar);
                return new PricingDisplayPlanDto(
                    p.Code,
                    p.Name,
                    amount,
                    p.MaxUsers);
            })
            .ToList();

        return new PricingDisplayDto(
            countryCode,
            currency,
            rate,
            rateSource,
            displayPlans);
    }

    private async Task<string?> ResolveCountryCodeAsync(
        string? clientIp,
        CancellationToken cancellationToken)
    {
        if (!TryGetPublicIp(clientIp, out var ip))
            return null;

        try
        {
            var client = httpClientFactory.CreateClient("GeoIp");
            using var response = await client.GetAsync(
                $"json/{ip}?fields=status,countryCode",
                cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var payload = await response.Content
                .ReadFromJsonAsync<IpApiResponse>(cancellationToken);
            if (payload is null
                || !string.Equals(payload.Status, "success", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(payload.CountryCode))
            {
                return null;
            }

            return payload.CountryCode.Trim().ToUpperInvariant();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Geo IP lookup failed for {Ip}", ip);
            return null;
        }
    }

    private async Task<(decimal Rate, string Source)> ResolveSarToEgpRateAsync(
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(FxCacheKey, out decimal cached) && cached > 0)
            return (cached, "cache");

        var live = await TryFetchLiveRateAsync(cancellationToken);
        if (live is > 0)
        {
            await PersistRateAsync(live.Value, cancellationToken);
            cache.Set(FxCacheKey, live.Value, FxMemoryTtl);
            return (live.Value, "live");
        }

        var stored = await db.FxRateSnapshots
            .AsNoTracking()
            .Where(x => x.FromCurrency == FromCurrency && x.ToCurrency == ToCurrency)
            .Select(x => (decimal?)x.Rate)
            .FirstOrDefaultAsync(cancellationToken);

        if (stored is > 0)
        {
            cache.Set(FxCacheKey, stored.Value, FxMemoryTtl);
            return (stored.Value, "stored");
        }

        await PersistRateAsync(SeedFallbackRate, cancellationToken);
        cache.Set(FxCacheKey, SeedFallbackRate, FxMemoryTtl);
        return (SeedFallbackRate, "seed");
    }

    private async Task<decimal?> TryFetchLiveRateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient("FxRates");
            using var response = await client.GetAsync("v6/latest/SAR", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var payload = await response.Content
                .ReadFromJsonAsync<OpenErApiResponse>(cancellationToken);
            if (payload?.Rates is null
                || !payload.Rates.TryGetValue(ToCurrency, out var egp)
                || egp <= 0)
            {
                return null;
            }

            return egp;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Live SAR→EGP FX fetch failed");
            return null;
        }
    }

    private async Task PersistRateAsync(decimal rate, CancellationToken cancellationToken)
    {
        var row = await db.FxRateSnapshots
            .FirstOrDefaultAsync(
                x => x.FromCurrency == FromCurrency && x.ToCurrency == ToCurrency,
                cancellationToken);

        if (row is null)
        {
            db.FxRateSnapshots.Add(new FxRateSnapshot
            {
                FromCurrency = FromCurrency,
                ToCurrency = ToCurrency,
                Rate = rate,
                FetchedAtUtc = DateTimeOffset.UtcNow
            });
        }
        else
        {
            row.Rate = rate;
            row.FetchedAtUtc = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool TryGetPublicIp(string? clientIp, out string ip)
    {
        ip = string.Empty;
        if (string.IsNullOrWhiteSpace(clientIp))
            return false;

        // X-Forwarded-For may be a list; take the first hop.
        var first = clientIp.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(first))
            return false;

        if (!IPAddress.TryParse(first, out var address))
            return false;

        if (IPAddress.IsLoopback(address)
            || address.Equals(IPAddress.Any)
            || address.Equals(IPAddress.IPv6Any)
            || address.IsIPv6LinkLocal
            || IsPrivate(address))
        {
            return false;
        }

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        ip = address.ToString();
        return true;
    }

    private static bool IsPrivate(IPAddress address)
    {
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] == 0xfc || bytes[0] == 0xfd || address.IsIPv6SiteLocal;
        }

        var b = address.GetAddressBytes();
        return b[0] == 10
            || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
            || (b[0] == 192 && b[1] == 168)
            || (b[0] == 169 && b[1] == 254);
    }

    private sealed class IpApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    }

    private sealed class OpenErApiResponse
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
