using CuttingOptimizer.Application.Services.Optimization;

namespace CuttingOptimizer.Application.Abstractions.Optimization;

public interface ICuttingPlanAlternativesGenerator
{
    IReadOnlyList<CuttingPlanAlternativeCandidate> Generate(
        int kerfMm,
        IReadOnlyList<OptimizationRequiredCut> requiredCuts,
        IReadOnlyList<OptimizationStockOption> stockOptions,
        bool useRemnants = true);
}

public sealed record CuttingPlanAlternativeCandidate(
    string StrategyCode,
    CuttingOptimizationResult Result,
    string BarsSummary);
