using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;
using CuttingOptimizer.Application.Services.Optimization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Queries.GetCuttingPlanComparison;

public sealed class GetCuttingPlanComparisonHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCuttingPlanComparisonQuery, CuttingPlanComparisonDto?>
{
    public async Task<CuttingPlanComparisonDto?> Handle(
        GetCuttingPlanComparisonQuery request,
        CancellationToken cancellationToken)
    {
        var plans = await context.CuttingPlans
            .AsNoTracking()
            .Include(plan => plan.Bars)
            .Where(plan => plan.ComparisonGroupId == request.ComparisonGroupId)
            .OrderBy(plan => plan.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (plans.Count == 0)
            return null;

        var summaries = plans
            .Select(plan =>
            {
                var fauxResult = new CuttingOptimizationResult(
                    plan.IsComplete,
                    plan.TotalBarsUsed,
                    plan.TotalMaterialCost,
                    plan.TotalKerfMm,
                    plan.TotalLeftoverMm,
                    plan.Bars
                        .Select(bar => new OptimizedBarAllocation(
                            bar.StockBarId,
                            bar.LengthMm,
                            bar.UnitPrice,
                            bar.KerfMm,
                            bar.RemainingLengthMm,
                            Array.Empty<AllocatedPiece>()))
                        .ToList(),
                    Array.Empty<UnallocatedPiece>(),
                    plan.StrategyNote);

                var summary = CuttingPlanMapper.ToSummary(
                    plan,
                    CuttingPlanAlternativesGenerator.BuildBarsSummary(fauxResult));
                return summary;
            })
            .ToList();

        var ordered = GenerateCuttingPlanHandler.OrderAlternatives(summaries);
        var complete = ordered.Where(a => a.IsComplete).ToList();

        Guid? recommendedCost = complete
            .OrderBy(a => a.TotalMaterialCost)
            .ThenBy(a => a.WastePercent)
            .Select(a => (Guid?)a.CuttingPlanId)
            .FirstOrDefault();

        Guid? recommendedWaste = complete
            .OrderBy(a => a.WastePercent)
            .ThenBy(a => a.TotalMaterialCost)
            .Select(a => (Guid?)a.CuttingPlanId)
            .FirstOrDefault();

        return new CuttingPlanComparisonDto(
            request.ComparisonGroupId,
            recommendedCost,
            recommendedWaste,
            ordered);
    }
}
