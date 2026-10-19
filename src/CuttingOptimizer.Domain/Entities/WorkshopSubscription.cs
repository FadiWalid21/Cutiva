using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class WorkshopSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkshopId { get; set; }

    public Guid PlanId { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public DateTime EndsAtUtc { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public string? Notes { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public decimal? PaymentAmount { get; set; }

    public string? PaymentCurrency { get; set; } = "SAR";

    public string? PaymentNote { get; set; }

    public DateTime? PaymentConfirmedAtUtc { get; set; }

    public Workshop Workshop { get; set; } = null!;

    public SubscriptionPlan Plan { get; set; } = null!;
}
