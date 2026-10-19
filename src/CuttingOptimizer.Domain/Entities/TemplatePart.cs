using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

/// <summary>
/// Aluminum part formula: LengthMm = (WidthCoef × W) + (HeightCoef × H) + OffsetMm.
/// Coefficients may use SashCount/OverlapMm from the variant via structured evaluation (no string eval).
/// </summary>
public class TemplatePart
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductVariantId { get; set; }

    public TemplatePartRole Role { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public int QtyPerUnit { get; set; } = 1;

    public decimal WidthCoef { get; set; }

    public decimal HeightCoef { get; set; }

    public int OffsetMm { get; set; }

    /// <summary>
    /// When true, width term uses (W / SashCount) + OverlapMm instead of raw W
    /// (structured sliding-sash rule; no string expressions).
    /// </summary>
    public bool UseSashWidthDivision { get; set; }

    public Guid? DefaultAluminumMaterialId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    public Material? DefaultAluminumMaterial { get; set; }

    public ICollection<TemplatePartAllowedMaterial> AllowedMaterials { get; set; }
        = new List<TemplatePartAllowedMaterial>();
}
