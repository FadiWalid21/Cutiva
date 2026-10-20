namespace CuttingOptimizer.Domain.Constants;

public static class PlanFeatureCodes
{
    public const string Resources = "resources";
    public const string ProductTemplates = "productTemplates";
    public const string Remnants = "remnants";
    public const string PlanComparisons = "planComparisons";
    public const string ClientQuotes = "clientQuotes";
    public const string WorkshopReports = "workshopReports";

    public static readonly IReadOnlyList<(string Code, string Name, int SortOrder)> Catalog =
    [
        (Resources, "Resources catalog", 10),
        (ProductTemplates, "Product templates", 20),
        (Remnants, "Remnant warehouse", 30),
        (PlanComparisons, "Plan comparisons", 40),
        (ClientQuotes, "Client quotes", 50),
        (WorkshopReports, "Workshop reports", 60)
    ];

    public static readonly IReadOnlyList<string> All =
        Catalog.Select(c => c.Code).ToList();
}
