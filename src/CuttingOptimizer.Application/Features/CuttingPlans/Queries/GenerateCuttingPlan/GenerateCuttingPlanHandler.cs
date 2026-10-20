using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Entitlements;
using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Application.Services.Optimization;
using CuttingOptimizer.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;

public sealed class GenerateCuttingPlanHandler(
    IApplicationDbContext context,
    ICuttingPlanAlternativesGenerator alternativesGenerator,
    IPlanEntitlementService entitlements)
    : IRequestHandler<GenerateCuttingPlanQuery, CuttingPlanComparisonDto?>
{
    public async Task<CuttingPlanComparisonDto?> Handle(
        GenerateCuttingPlanQuery request,
        CancellationToken cancellationToken)
    {
        var materialExists = await context.Materials
            .AsNoTracking()
            .AnyAsync(
                material => material.Id == request.MaterialId,
                cancellationToken);

        if (!materialExists)
            return null;

        var distinctCutIds = request.RequiredCutIds
            .Distinct()
            .ToList();

        var cutContexts = await context.RequiredCuts
            .AsNoTracking()
            .Where(cut =>
                distinctCutIds.Contains(cut.Id)
                && cut.MaterialId == request.MaterialId)
            .Select(cut => new
            {
                cut.Id,
                cut.Name,
                cut.LengthMm,
                cut.Quantity,
                cut.ProjectSection.ProjectId,
                cut.ProjectSection.Project.WorkshopId
            })
            .ToListAsync(cancellationToken);

        if (cutContexts.Count != distinctCutIds.Count)
            return null;

        var workshopIds = cutContexts
            .Select(cut => cut.WorkshopId)
            .Distinct()
            .ToList();

        if (workshopIds.Count != 1)
            return null;

        var workshopId = workshopIds[0];

        var canUseRemnants = await entitlements.HasAsync(
            workshopId,
            PlanFeatureCodes.Remnants,
            cancellationToken);
        var canCompare = await entitlements.HasAsync(
            workshopId,
            PlanFeatureCodes.PlanComparisons,
            cancellationToken);

        var useRemnants = request.UseRemnants && canUseRemnants;

        var projectIds = cutContexts
            .Select(cut => cut.ProjectId)
            .Distinct()
            .ToList();

        Guid? projectId = projectIds.Count == 1 ? projectIds[0] : null;

        var requiredCuts = cutContexts
            .Select(cut => new OptimizationRequiredCut(
                cut.Id,
                cut.Name,
                cut.LengthMm,
                cut.Quantity))
            .ToList();

        // Disable free catalog SKUs so they never enter FFD as "market price 0".
        var freeCatalog = await context.StockBars
            .Where(stockBar =>
                stockBar.MaterialId == request.MaterialId
                && stockBar.IsActive
                && !stockBar.IsRemnant
                && stockBar.UnitPrice <= 0)
            .ToListAsync(cancellationToken);

        if (freeCatalog.Count > 0)
        {
            foreach (var bar in freeCatalog)
                bar.IsActive = false;

            await context.SaveChangesAsync(cancellationToken);
        }

        // Catalog: unlimited supply. Remnants: limited qty, preferred when allowed.
        var stockQuery = context.StockBars
            .AsNoTracking()
            .Where(stockBar =>
                stockBar.MaterialId == request.MaterialId
                && stockBar.IsActive
                && (
                    !stockBar.IsRemnant
                    || stockBar.AvailableQuantity > 0));

        if (!useRemnants)
            stockQuery = stockQuery.Where(stockBar => !stockBar.IsRemnant);

        // Catalog bars must be priced; free catalog SKUs look like remnants in the buy list.
        stockQuery = stockQuery.Where(stockBar =>
            stockBar.IsRemnant || stockBar.UnitPrice > 0);

        var stockOptions = await stockQuery
            .Select(stockBar => new OptimizationStockOption(
                stockBar.Id,
                stockBar.LengthMm,
                stockBar.IsRemnant ? 0m : stockBar.UnitPrice,
                stockBar.IsRemnant
                    ? stockBar.AvailableQuantity
                    : int.MaxValue / 4,
                stockBar.IsRemnant))
            .ToListAsync(cancellationToken);

        var candidates = alternativesGenerator.Generate(
            request.KerfMm,
            requiredCuts,
            stockOptions,
            useRemnants);

        if (!canCompare && candidates.Count > 1)
        {
            candidates = candidates
                .OrderBy(c => c.Result.TotalMaterialCost)
                .ThenBy(c => c.Result.TotalLeftoverMm + c.Result.TotalKerfMm)
                .Take(1)
                .ToList();
        }

        var comparisonGroupId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;
        var summaries = new List<CuttingPlanAlternativeSummaryDto>();

        foreach (var candidate in candidates)
        {
            var plan = CuttingPlanMapper.FromResult(
                candidate.Result,
                workshopId,
                request.MaterialId,
                projectId,
                request.KerfMm,
                comparisonGroupId,
                candidate.StrategyCode,
                createdAtUtc,
                useRemnants);

            context.CuttingPlans.Add(plan);
            summaries.Add(CuttingPlanMapper.ToSummary(plan, candidate.BarsSummary));
        }

        await context.SaveChangesAsync(cancellationToken);

        var ordered = OrderAlternatives(summaries);
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
            comparisonGroupId,
            recommendedCost,
            recommendedWaste,
            ordered);
    }

    internal static IReadOnlyList<CuttingPlanAlternativeSummaryDto> OrderAlternatives(
        IReadOnlyList<CuttingPlanAlternativeSummaryDto> summaries) =>
        summaries
            .OrderByDescending(a => a.IsComplete)
            .ThenBy(a => a.TotalMaterialCost)
            .ThenBy(a => a.WastePercent)
            .ThenBy(a => a.StrategyCode)
            .ToList();
}
