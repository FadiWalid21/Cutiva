using CuttingOptimizer.Domain.Entities;

namespace CuttingOptimizer.Application.Features.CuttingPlans;

internal static class CuttingPlanMapper
{
    public static CuttingPlanDto ToDto(CuttingPlan plan) =>
        new(
            plan.Id,
            plan.WorkshopId,
            plan.MaterialId,
            plan.ProjectId,
            plan.KerfMm,
            plan.IsComplete,
            plan.Status.ToString(),
            plan.ApprovedAtUtc,
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
