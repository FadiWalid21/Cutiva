namespace CuttingOptimizer.Domain.Entities;

public class FeatureDefinition
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public ICollection<SubscriptionPlanFeature> PlanFeatures { get; set; }
        = new List<SubscriptionPlanFeature>();
}
