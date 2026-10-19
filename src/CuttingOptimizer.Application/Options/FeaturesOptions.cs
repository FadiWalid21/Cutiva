namespace CuttingOptimizer.Application.Options;

public sealed class FeaturesOptions
{
    public const string SectionName = "Features";

    public bool ResourcesV2 { get; set; }

    public bool ProductTemplates { get; set; }
}
