namespace CuttingOptimizer.Domain.Entities;

public class SubscriptionPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Stable code: Starter, Workshop, Studio.</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal PriceSar { get; set; }

    public int DurationDays { get; set; } = 30;

    /// <summary>Null = unlimited seats.</summary>
    public int? MaxUsers { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<WorkshopSubscription> Subscriptions { get; set; }
        = new List<WorkshopSubscription>();

    public ICollection<SubscriptionPlanFeature> Features { get; set; }
        = new List<SubscriptionPlanFeature>();
}
