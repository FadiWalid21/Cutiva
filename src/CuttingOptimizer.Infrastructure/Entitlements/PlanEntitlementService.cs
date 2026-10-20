using CuttingOptimizer.Application.Abstractions.Entitlements;
using CuttingOptimizer.Domain.Enums;
using CuttingOptimizer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Infrastructure.Entitlements;

public sealed class PlanEntitlementService(ApplicationDbContext db) : IPlanEntitlementService
{
    public async Task<IReadOnlyList<string>> GetEnabledFeaturesAsync(
        Guid? workshopId,
        CancellationToken cancellationToken = default)
    {
        if (!workshopId.HasValue)
            return [];

        var now = DateTime.UtcNow;
        var planId = await db.WorkshopSubscriptions
            .AsNoTracking()
            .Where(s => s.WorkshopId == workshopId.Value)
            .OrderByDescending(s => s.EndsAtUtc)
            .Select(s => new { s.PlanId, s.Status, s.EndsAtUtc })
            .FirstOrDefaultAsync(cancellationToken);

        if (planId is null)
            return [];

        // Suspended plans grant nothing; expired Active still read matrix but callers
        // already surface subscription status in the shell.
        if (planId.Status == SubscriptionStatus.Suspended)
            return [];

        return await db.SubscriptionPlanFeatures
            .AsNoTracking()
            .Where(f => f.PlanId == planId.PlanId && f.IsEnabled)
            .Select(f => f.FeatureCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasAsync(
        Guid? workshopId,
        string featureCode,
        CancellationToken cancellationToken = default)
    {
        if (!workshopId.HasValue || string.IsNullOrWhiteSpace(featureCode))
            return false;

        var enabled = await GetEnabledFeaturesAsync(workshopId, cancellationToken);
        return enabled.Contains(featureCode, StringComparer.Ordinal);
    }
}
