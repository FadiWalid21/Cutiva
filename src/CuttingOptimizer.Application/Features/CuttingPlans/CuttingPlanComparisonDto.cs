namespace CuttingOptimizer.Application.Features.CuttingPlans;

public sealed record CuttingPlanComparisonDto(
    Guid ComparisonGroupId,
    Guid? RecommendedCostPlanId,
    Guid? RecommendedWastePlanId,
    IReadOnlyList<CuttingPlanAlternativeSummaryDto> Alternatives);

public sealed record CuttingPlanAlternativeSummaryDto(
    Guid CuttingPlanId,
    string StrategyCode,
    bool IsComplete,
    int TotalBarsUsed,
    decimal TotalMaterialCost,
    int TotalPieceLengthMm,
    int TotalStockLengthMm,
    int TotalLeftoverMm,
    double WastePercent,
    string BarsSummary,
    string StrategyNote);
