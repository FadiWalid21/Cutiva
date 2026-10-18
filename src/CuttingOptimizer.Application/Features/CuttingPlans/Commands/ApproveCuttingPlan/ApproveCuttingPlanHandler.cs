using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Commands.ApproveCuttingPlan;

public sealed class ApproveCuttingPlanHandler(
    IApplicationDbContext context)
    : IRequestHandler<ApproveCuttingPlanCommand, ApproveCuttingPlanResult>
{
    public async Task<ApproveCuttingPlanResult> Handle(
        ApproveCuttingPlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await context.CuttingPlans
            .Include(p => p.Bars)
                .ThenInclude(b => b.Pieces)
            .Include(p => p.UnallocatedPieces)
            .FirstOrDefaultAsync(
                p => p.Id == request.CuttingPlanId,
                cancellationToken);

        if (plan is null)
            return new ApproveCuttingPlanNotFound();

        if (plan.Status == CuttingPlanStatus.Approved)
            return new ApproveCuttingPlanAlreadyApproved();

        var usageByStockBarId = plan.Bars
            .GroupBy(bar => bar.StockBarId)
            .ToDictionary(group => group.Key, group => group.Count());

        var stockBarIds = usageByStockBarId.Keys.ToList();
        var stockBars = await context.StockBars
            .Where(stock => stockBarIds.Contains(stock.Id))
            .ToListAsync(cancellationToken);

        if (stockBars.Count != stockBarIds.Count)
            return new ApproveCuttingPlanInsufficientStock();

        foreach (var stockBar in stockBars)
        {
            var needed = usageByStockBarId[stockBar.Id];
            if (!stockBar.IsActive || stockBar.AvailableQuantity < needed)
                return new ApproveCuttingPlanInsufficientStock();
        }

        foreach (var stockBar in stockBars)
        {
            stockBar.AvailableQuantity -= usageByStockBarId[stockBar.Id];
        }

        foreach (var planBar in plan.Bars.Where(bar => bar.RemainingLengthMm > 0))
        {
            var source = stockBars.First(stock => stock.Id == planBar.StockBarId);
            context.StockBars.Add(new StockBar
            {
                MaterialId = plan.MaterialId,
                LengthMm = planBar.RemainingLengthMm,
                UnitPrice = planBar.UnitPrice > 0 ? planBar.UnitPrice : source.UnitPrice,
                AvailableQuantity = 1,
                IsActive = true
            });
        }

        plan.Status = CuttingPlanStatus.Approved;
        plan.ApprovedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new ApproveCuttingPlanSucceeded(CuttingPlanMapper.ToDto(plan));
    }
}
