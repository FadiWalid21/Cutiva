namespace CuttingOptimizer.Application.Features.Projects.Commands.CalculateProject;

public sealed record CalculateProjectResultDto(
    Guid ProjectId,
    decimal TotalAluminumCost,
    decimal TotalGlassCost,
    decimal TotalAccessoryCost,
    decimal GrandTotal,
    IReadOnlyList<PurchaseBarLineDto> PurchaseBars,
    IReadOnlyList<CalculateGlassPaneDto> GlassPanes,
    IReadOnlyList<CalculateAccessoryLineDto> Accessories,
    IReadOnlyList<CalculateItemCostDto> ItemCosts,
    IReadOnlyList<CalculateUnallocatedDto> Unallocated,
    IReadOnlyList<CalculateMaterialWasteDto> WasteByMaterial);

public sealed record PurchaseBarLineDto(
    Guid MaterialId,
    string MaterialCode,
    string MaterialName,
    int LengthMm,
    int Quantity,
    decimal UnitPrice,
    decimal LineCost,
    bool IsRemnant);

public sealed record CalculateGlassPaneDto(
    Guid ProjectItemId,
    string ItemLabel,
    Guid GlassSpecId,
    string Name,
    int WidthMm,
    int HeightMm,
    decimal AreaM2PerPane,
    int PaneQuantity,
    decimal PricePerSqm,
    decimal LineCost);

public sealed record CalculateAccessoryLineDto(
    Guid ProjectItemId,
    string ItemLabel,
    Guid AccessoryId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal LineCost);

public sealed record CalculateItemCostDto(
    Guid ProjectItemId,
    string Label,
    int EffectiveWidthMm,
    int EffectiveHeightMm,
    decimal AluminumCost,
    decimal GlassCost,
    decimal AccessoryCost,
    decimal TotalCost);

public sealed record CalculateUnallocatedDto(
    Guid MaterialId,
    string MaterialCode,
    string Name,
    int LengthMm,
    int Quantity);

public sealed record CalculateMaterialWasteDto(
    Guid MaterialId,
    string MaterialCode,
    int TotalKerfMm,
    int TotalLeftoverMm,
    decimal WastePercent,
    bool IsComplete);
