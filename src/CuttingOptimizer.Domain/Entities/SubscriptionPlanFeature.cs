namespace CuttingOptimizer.Domain.Entities;

public class SubscriptionPlanFeature
{
    public Guid PlanId { get; set; }

    public string FeatureCode { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public SubscriptionPlan Plan { get; set; } = null!;

    public FeatureDefinition Feature { get; set; } = null!;
}
