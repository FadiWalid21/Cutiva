using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.CuttingPlans;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Queries.GetCuttingPlanById;

public sealed class GetCuttingPlanByIdHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetCuttingPlanByIdQuery, CuttingPlanDto?>
{
    public async Task<CuttingPlanDto?> Handle(
        GetCuttingPlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.CuttingPlans
            .AsNoTracking()
            .Where(plan => plan.Id == request.Id)
            .Select(plan => new CuttingPlanDto(
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
                plan.StrategyNote))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
