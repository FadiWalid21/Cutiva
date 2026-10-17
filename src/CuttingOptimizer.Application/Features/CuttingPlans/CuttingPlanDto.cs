namespace CuttingOptimizer.Application.Features.CuttingPlans;

public sealed record CuttingPlanDto(
    bool IsComplete,
    int TotalBarsUsed,
    decimal TotalMaterialCost,
    int TotalKerfMm,
    int TotalLeftoverMm,
    IReadOnlyList<CuttingPlanBarDto> Bars,
    IReadOnlyList<CuttingPlanUnallocatedPieceDto> UnallocatedPieces,
    string StrategyNote);

public sealed record CuttingPlanBarDto(
    Guid StockBarId,
    int LengthMm,
    decimal UnitPrice,
    int KerfMm,
    int RemainingLengthMm,
    IReadOnlyList<CuttingPlanPieceDto> Pieces);

public sealed record CuttingPlanPieceDto(
    Guid RequiredCutId,
    string Name,
    int LengthMm);

public sealed record CuttingPlanUnallocatedPieceDto(
    Guid RequiredCutId,
    string Name,
    int LengthMm);
