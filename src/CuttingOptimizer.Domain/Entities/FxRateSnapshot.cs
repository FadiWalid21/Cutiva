namespace CuttingOptimizer.Domain.Entities;

/// <summary>Last known FX rate used when live quotes are unavailable.</summary>
public class FxRateSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FromCurrency { get; set; } = string.Empty;

    public string ToCurrency { get; set; } = string.Empty;

    public decimal Rate { get; set; }

    public DateTimeOffset FetchedAtUtc { get; set; }
}
