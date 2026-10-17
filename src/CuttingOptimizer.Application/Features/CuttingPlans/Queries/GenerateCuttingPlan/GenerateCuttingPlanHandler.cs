using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Application.Services.Optimization;
using CuttingOptimizer.Domain.Entities;
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

        var totalPieceLengthMm = result.Bars
            .SelectMany(bar => bar.Pieces)
            .Sum(piece => piece.LengthMm);

        var totalStockLengthMm = result.Bars.Sum(bar => bar.LengthMm);
        var createdAtUtc = DateTime.UtcNow;

        var plan = new CuttingPlan
        {
            WorkshopId = workshopId,
            MaterialId = request.MaterialId,
            ProjectId = projectId,
            KerfMm = request.KerfMm,
            IsComplete = result.IsComplete,
            TotalPieceLengthMm = totalPieceLengthMm,
            TotalStockLengthMm = totalStockLengthMm,
            TotalBarsUsed = result.TotalBarsUsed,
            TotalMaterialCost = result.TotalMaterialCost,
            TotalKerfMm = result.TotalKerfMm,
            TotalLeftoverMm = result.TotalLeftoverMm,
            CreatedAtUtc = createdAtUtc,
            StrategyNote = result.StrategyNote
        };

        foreach (var bar in result.Bars)
        {
            var planBar = new CuttingPlanBar
            {
                StockBarId = bar.StockBarId,
                LengthMm = bar.LengthMm,
                UnitPrice = bar.UnitPrice,
                KerfMm = bar.KerfMm,
                RemainingLengthMm = bar.RemainingLengthMm
            };

            foreach (var piece in bar.Pieces)
            {
                planBar.Pieces.Add(new CuttingPlanPiece
                {
                    RequiredCutId = piece.RequiredCutId,
                    Name = piece.Name,
                    LengthMm = piece.LengthMm
                });
            }

            plan.Bars.Add(planBar);
        }

        foreach (var piece in result.UnallocatedPieces)
        {
            plan.UnallocatedPieces.Add(new CuttingPlanUnallocatedPiece
            {
                RequiredCutId = piece.RequiredCutId,
                Name = piece.Name,
                LengthMm = piece.LengthMm
            });
        }

        // Calculation-only persistence: inventory quantities are not changed.
        context.CuttingPlans.Add(plan);
        await context.SaveChangesAsync(cancellationToken);

        return MapToDto(plan);
    }

    private static CuttingPlanDto MapToDto(CuttingPlan plan) =>
        new(
            plan.Id,
            plan.WorkshopId,
            plan.MaterialId,
            plan.ProjectId,
            plan.KerfMm,
            plan.IsComplete,
            plan.TotalPieceLengthMm,
            plan.TotalStockLengthMm,
            plan.TotalBarsUsed,
            plan.TotalMaterialCost,
            plan.TotalKerfMm,
            plan.TotalLeftoverMm,
            plan.CreatedAtUtc,
            plan.Bars
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
            plan.UnallocatedPieces
                .Select(piece => new CuttingPlanUnallocatedPieceDto(
                    piece.RequiredCutId,
                    piece.Name,
                    piece.LengthMm))
                .ToList(),
            plan.StrategyNote);
}
