namespace CuttingOptimizer.Application.Features.Pricing;

public sealed record PricingDisplayPlanDto(
    string Code,
    string Name,
    decimal Price,
    int? MaxUsers);

public sealed record PricingDisplayDto(
    string? CountryCode,
    string Currency,
    decimal? RateSarToEgp,
    string RateSource,
    IReadOnlyList<PricingDisplayPlanDto> Plans);
