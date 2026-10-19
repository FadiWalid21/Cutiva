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

        if (!plan.IsComplete || plan.UnallocatedPieces.Count > 0)
            return new ApproveCuttingPlanIncomplete();

        var usedStockBarIds = plan.Bars
            .Select(bar => bar.StockBarId)
            .Distinct()
            .ToList();

        var usedStockBars = await context.StockBars
            .Where(stockBar => usedStockBarIds.Contains(stockBar.Id))
            .ToListAsync(cancellationToken);

        var remnantById = usedStockBars
            .Where(stockBar => stockBar.IsRemnant)
            .ToDictionary(stockBar => stockBar.Id);

        // Consume remnant inventory used by this plan (catalog is unlimited).
        foreach (var group in plan.Bars.GroupBy(bar => bar.StockBarId))
        {
            if (!remnantById.TryGetValue(group.Key, out var remnant))
                continue;

            var usedCount = group.Count();
            remnant.AvailableQuantity = Math.Max(0, remnant.AvailableQuantity - usedCount);
            if (remnant.AvailableQuantity == 0)
                remnant.IsActive = false;
        }

        // Store usable leftovers as remnant stock for future plans.
        foreach (var bar in plan.Bars.Where(b => b.RemainingLengthMm > 0))
        {
            context.StockBars.Add(new StockBar
            {
                MaterialId = plan.MaterialId,
                LengthMm = bar.RemainingLengthMm,
                UnitPrice = 0m,
                AvailableQuantity = 1,
                IsActive = true,
                IsRemnant = true
            });
        }

        plan.Status = CuttingPlanStatus.Approved;
        plan.ApprovedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new ApproveCuttingPlanSucceeded(CuttingPlanMapper.ToDto(plan));
    }
}
