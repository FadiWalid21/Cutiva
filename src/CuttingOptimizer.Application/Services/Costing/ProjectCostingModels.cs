using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Services.Costing;

/// <summary>
/// One project item with template-driven parts, glass slots, and accessories
/// ready for BOM explosion and costing (no database types).
/// </summary>
public sealed record CostingProjectItemInput(
    Guid ProjectItemId,
    int WidthMm,
    int HeightMm,
    int Quantity,
    int? SashCount,
    int? OverlapMm,
    IReadOnlyList<CostingAluminumPartInput> AluminumParts,
    IReadOnlyList<CostingGlassSlotInput> GlassSlots,
    IReadOnlyList<CostingAccessorySlotInput> AccessorySlots);

public sealed record CostingAluminumPartInput(
    Guid TemplatePartId,
    TemplatePartRole Role,
    string DisplayName,
    int QtyPerUnit,
    decimal WidthCoef,
    decimal HeightCoef,
    int OffsetMm,
    bool UseSashWidthDivision,
    Guid MaterialId);

public sealed record CostingGlassSlotInput(
    Guid TemplateGlassSlotId,
    string Name,
    int PaneCount,
    decimal WidthCoefW,
    decimal HeightCoefW,
    int OffsetWMm,
    decimal WidthCoefH,
    decimal HeightCoefH,
    int OffsetHMm,
    bool UseSashWidthDivision,
    Guid GlassSpecId,
    decimal PricePerSqm);

public sealed record CostingAccessorySlotInput(
    Guid TemplateAccessorySlotId,
    string Name,
    AccessoryQuantityRule QuantityRule,
    int Qty,
    Guid AccessoryId,
    decimal UnitPrice);

/// <summary>
/// Packing outcome for one aluminum material after the optimizer runs.
/// <see cref="TotalConsumedLengthMm"/> is the sum of bar lengths used
/// (piece length + kerf + leftover / waste). <see cref="TotalCost"/> is the
/// purchase cost of those bars.
/// </summary>
public sealed record MaterialPackingCost(
    Guid MaterialId,
    decimal TotalCost,
    int TotalConsumedLengthMm);

public sealed record BomAluminumPiece(
    Guid ProjectItemId,
    Guid TemplatePartId,
    TemplatePartRole Role,
    Guid MaterialId,
    string DisplayName,
    int LengthMm,
    int Quantity);

public sealed record BomGlassPane(
    Guid ProjectItemId,
    Guid TemplateGlassSlotId,
    Guid GlassSpecId,
    string Name,
    int WidthMm,
    int HeightMm,
    decimal AreaM2PerPane,
    int PaneQuantity,
    decimal PricePerSqm,
    decimal LineCost);

public sealed record BomAccessoryLine(
    Guid ProjectItemId,
    Guid TemplateAccessorySlotId,
    Guid AccessoryId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal LineCost);

public sealed record BomExplodeResult(
    IReadOnlyList<BomAluminumPiece> AluminumPieces,
    IReadOnlyList<BomGlassPane> GlassPanes,
    IReadOnlyList<BomAccessoryLine> Accessories);

/// <summary>
/// Aluminum material cost attributed to one project item after packing.
/// Waste is included proportionally via share of piece length (see calculator docs).
/// </summary>
public sealed record ItemAluminumCostAllocation(
    Guid ProjectItemId,
    Guid MaterialId,
    int PieceLengthMm,
    decimal AllocatedCost);

public sealed record ProjectItemCostSummary(
    Guid ProjectItemId,
    decimal AluminumCost,
    decimal GlassCost,
    decimal AccessoryCost,
    decimal TotalCost);

public sealed record ProjectCostResult(
    BomExplodeResult Bom,
    IReadOnlyList<ItemAluminumCostAllocation> AluminumAllocations,
    IReadOnlyList<ProjectItemCostSummary> ItemSummaries,
    decimal TotalAluminumCost,
    decimal TotalGlassCost,
    decimal TotalAccessoryCost,
    decimal GrandTotal);

/// <summary>
/// Thrown when a structured length/size formula yields a non-positive result.
/// </summary>
public sealed class ProjectCostingValidationException : Exception
{
    public ProjectCostingValidationException(string message)
        : base(message)
    {
    }
}
