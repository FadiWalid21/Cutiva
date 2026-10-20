namespace CuttingOptimizer.Application.Abstractions.Entitlements;

public interface IPlanEntitlementService
{
    /// <summary>
    /// Enabled feature codes for the workshop's current subscription plan.
    /// Empty when there is no workshop or no plan row.
    /// </summary>
    Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(
        Guid? workshopId,
        CancellationToken cancellationToken = default);

    Task<bool> HasAsync(
        Guid? workshopId,
        string featureCode,
        CancellationToken cancellationToken = default);
}
