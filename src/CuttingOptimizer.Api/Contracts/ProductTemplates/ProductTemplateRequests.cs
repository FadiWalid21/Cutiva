using System.ComponentModel.DataAnnotations;
using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Api.Contracts.ProductTemplates;

public sealed record UpdateProductVariantRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name,

    int? SashCount,

    int? OverlapMm,

    bool IsActive = true);

public sealed record UpdateTemplatePartRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string DisplayName,

    [Range(1, int.MaxValue)]
    int QtyPerUnit,

    decimal WidthCoef,

    decimal HeightCoef,

    int OffsetMm,

    bool UseSashWidthDivision,

    Guid? DefaultAluminumMaterialId);

public sealed record UpdateTemplateGlassSlotRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name,

    [Range(1, int.MaxValue)]
    int PaneCount,

    decimal WidthCoefW,

    decimal HeightCoefW,

    int OffsetWMm,

    decimal WidthCoefH,

    decimal HeightCoefH,

    int OffsetHMm,

    bool Required,

    bool UseSashWidthDivision);

public sealed record UpdateTemplateAccessorySlotRequest(
    [Required]
    [StringLength(150, MinimumLength = 1)]
    string Name,

    AccessoryQuantityRule QuantityRule,

    [Range(1, int.MaxValue)]
    int Qty,

    Guid? DefaultAccessoryId);
