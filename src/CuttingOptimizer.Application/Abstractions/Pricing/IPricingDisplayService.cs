using CuttingOptimizer.Application.Features.Pricing;

namespace CuttingOptimizer.Application.Abstractions.Pricing;

public interface IPricingDisplayService
{
    Task<PricingDisplayDto> GetDisplayAsync(
        string? clientIp,
        CancellationToken cancellationToken = default);
}
