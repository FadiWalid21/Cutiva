using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Infrastructure.Auth;

internal static class SubscriptionSummaryFactory
{
    public static SubscriptionSummaryDto? FromCurrent(
        WorkshopSubscription? subscription,
        DateTime utcNow)
    {
        if (subscription is null)
            return null;

        var status = subscription.Status;
        if (status == SubscriptionStatus.Active && subscription.EndsAtUtc < utcNow)
            status = SubscriptionStatus.Expired;

        var days = (int)Math.Ceiling((subscription.EndsAtUtc - utcNow).TotalDays);
        if (days < 0)
            days = 0;

        return new SubscriptionSummaryDto(
            subscription.Plan.Code,
            subscription.Plan.Name,
            subscription.StartsAtUtc,
            subscription.EndsAtUtc,
            status.ToString(),
            days,
            subscription.Plan.MaxUsers,
            null);
    }

    public static SubscriptionSummaryDto WithSeats(
        SubscriptionSummaryDto? summary,
        int seatUsed)
    {
        if (summary is null)
            return null!;

        return summary with { SeatUsed = seatUsed };
    }
}
