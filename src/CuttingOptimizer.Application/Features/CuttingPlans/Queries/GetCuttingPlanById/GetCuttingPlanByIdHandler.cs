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
        var plan = await context.CuttingPlans
            .AsNoTracking()
            .Include(p => p.Bars)
                .ThenInclude(b => b.Pieces)
            .Include(p => p.Bars)
                .ThenInclude(b => b.StockBar)
            .Include(p => p.UnallocatedPieces)
            .FirstOrDefaultAsync(
                p => p.Id == request.Id,
                cancellationToken);

        return plan is null ? null : CuttingPlanMapper.ToDto(plan);
    }
}
