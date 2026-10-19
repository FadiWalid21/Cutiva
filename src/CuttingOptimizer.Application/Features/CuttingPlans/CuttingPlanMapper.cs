using CuttingOptimizer.Application.Services.Optimization;
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
            plan.ComparisonGroupId,
            plan.StrategyCode,
            plan.KerfMm,
            plan.UseRemnants,
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
                    // Prefer StockBar flag; fall back to zero-price (optimizer zeros remnant cost).
                    bar.StockBar?.IsRemnant == true || bar.UnitPrice <= 0,
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

    public static CuttingPlanAlternativeSummaryDto ToSummary(
        CuttingPlan plan,
        string barsSummary) =>
        new(
            plan.Id,
            plan.StrategyCode,
            plan.IsComplete,
            plan.TotalBarsUsed,
            plan.TotalMaterialCost,
            plan.TotalPieceLengthMm,
            plan.TotalStockLengthMm,
            plan.TotalLeftoverMm,
            WastePercent(plan.TotalLeftoverMm, plan.TotalStockLengthMm),
            barsSummary,
            plan.StrategyNote);

    public static CuttingPlan FromResult(
        CuttingOptimizationResult result,
        Guid workshopId,
        Guid materialId,
        Guid? projectId,
        int kerfMm,
        Guid comparisonGroupId,
        string strategyCode,
        DateTime createdAtUtc,
        bool useRemnants = true)
    {
        var totalPieceLengthMm = result.Bars
            .SelectMany(bar => bar.Pieces)
            .Sum(piece => piece.LengthMm);

        var totalStockLengthMm = result.Bars.Sum(bar => bar.LengthMm);

        var plan = new CuttingPlan
        {
            WorkshopId = workshopId,
            MaterialId = materialId,
            ProjectId = projectId,
            ComparisonGroupId = comparisonGroupId,
            StrategyCode = strategyCode,
            KerfMm = kerfMm,
            UseRemnants = useRemnants,
            IsComplete = result.IsComplete,
            Status = Domain.Enums.CuttingPlanStatus.Draft,
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

        return plan;
    }

    private static double WastePercent(int leftoverMm, int stockMm) =>
        stockMm <= 0 ? 0 : Math.Round(leftoverMm * 100.0 / stockMm, 1);
}
