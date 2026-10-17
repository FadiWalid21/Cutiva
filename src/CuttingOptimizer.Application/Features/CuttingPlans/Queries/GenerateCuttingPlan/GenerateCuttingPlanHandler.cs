using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Application.Services.Optimization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;

public sealed class GenerateCuttingPlanHandler(
    IApplicationDbContext context,
    ICuttingPlanOptimizer optimizer)
    : IRequestHandler<GenerateCuttingPlanQuery, CuttingPlanDto?>
{
    public async Task<CuttingPlanDto?> Handle(
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

        var requiredCuts = await context.RequiredCuts
            .AsNoTracking()
            .Where(cut =>
                distinctCutIds.Contains(cut.Id)
                && cut.MaterialId == request.MaterialId)
            .Select(cut => new OptimizationRequiredCut(
                cut.Id,
                cut.Name,
                cut.LengthMm,
                cut.Quantity))
            .ToListAsync(cancellationToken);

        if (requiredCuts.Count != distinctCutIds.Count)
            return null;

        var stockOptions = await context.StockBars
            .AsNoTracking()
            .Where(stockBar =>
                stockBar.MaterialId == request.MaterialId
                && stockBar.IsActive
                && stockBar.AvailableQuantity > 0)
            .Select(stockBar => new OptimizationStockOption(
                stockBar.Id,
                stockBar.LengthMm,
                stockBar.UnitPrice,
                stockBar.AvailableQuantity))
            .ToListAsync(cancellationToken);

        var result = optimizer.Optimize(new CuttingOptimizationInput(
            request.KerfMm,
            requiredCuts,
            stockOptions));

        return new CuttingPlanDto(
            result.IsComplete,
            result.TotalBarsUsed,
            result.TotalMaterialCost,
            result.TotalKerfMm,
            result.TotalLeftoverMm,
            result.Bars
                .Select(bar => new CuttingPlanBarDto(
                    bar.StockBarId,
                    bar.LengthMm,
                    bar.UnitPrice,
                    bar.KerfMm,
                    bar.RemainingLengthMm,
                    bar.Pieces
                        .Select(piece => new CuttingPlanPieceDto(
                            piece.RequiredCutId,
                            piece.Name,
                            piece.LengthMm))
                        .ToList()))
                .ToList(),
            result.UnallocatedPieces
                .Select(piece => new CuttingPlanUnallocatedPieceDto(
                    piece.RequiredCutId,
                    piece.Name,
                    piece.LengthMm))
                .ToList(),
            result.StrategyNote);
    }
}
