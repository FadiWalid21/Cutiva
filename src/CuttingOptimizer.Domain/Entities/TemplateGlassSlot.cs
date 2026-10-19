namespace CuttingOptimizer.Domain.Entities;

/// <summary>
/// GlassW = (WidthCoefW × W) + (HeightCoefW × H) + OffsetWMm
/// GlassH = (WidthCoefH × W) + (HeightCoefH × H) + OffsetHMm
/// </summary>
public class TemplateGlassSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductVariantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int PaneCount { get; set; } = 1;

    public decimal WidthCoefW { get; set; } = 1m;

    public decimal HeightCoefW { get; set; }

    public int OffsetWMm { get; set; }

    public decimal WidthCoefH { get; set; }

    public decimal HeightCoefH { get; set; } = 1m;

    public int OffsetHMm { get; set; }

    public bool Required { get; set; } = true;

    public bool UseSashWidthDivision { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;
}
