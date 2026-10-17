namespace CuttingOptimizer.Application.Services.Optimization;

public sealed record CuttingOptimizationInput(
    int KerfMm,
    IReadOnlyList<OptimizationRequiredCut> RequiredCuts,
    IReadOnlyList<OptimizationStockOption> StockOptions);

public sealed record OptimizationRequiredCut(
    Guid RequiredCutId,
    string Name,
    int LengthMm,
    int Quantity);

public sealed record OptimizationStockOption(
    Guid StockBarId,
    int LengthMm,
    decimal UnitPrice,
    int AvailableQuantity);

public sealed record CuttingOptimizationResult(
    bool IsComplete,
    int TotalBarsUsed,
    decimal TotalMaterialCost,
    int TotalKerfMm,
    int TotalLeftoverMm,
    IReadOnlyList<OptimizedBarAllocation> Bars,
    IReadOnlyList<UnallocatedPiece> UnallocatedPieces,
    string StrategyNote);

public sealed record OptimizedBarAllocation(
    Guid StockBarId,
    int LengthMm,
    decimal UnitPrice,
    int KerfMm,
    int RemainingLengthMm,
    IReadOnlyList<AllocatedPiece> Pieces);

public sealed record AllocatedPiece(
    Guid RequiredCutId,
    string Name,
    int LengthMm);

public sealed record UnallocatedPiece(
    Guid RequiredCutId,
    string Name,
    int LengthMm);
