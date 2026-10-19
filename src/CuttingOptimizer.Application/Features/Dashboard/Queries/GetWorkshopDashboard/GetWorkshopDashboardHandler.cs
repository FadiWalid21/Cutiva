using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Dashboard;
using CuttingOptimizer.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Dashboard.Queries.GetWorkshopDashboard;

public sealed class GetWorkshopDashboardHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetWorkshopDashboardQuery, WorkshopDashboardDto?>
{
    private const int RecentDays = 30;
    private const int RecentProjectsLimit = 20;

    public async Task<WorkshopDashboardDto?> Handle(
        GetWorkshopDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var workshopExists = await context.Workshops
            .AsNoTracking()
            .AnyAsync(
                workshop => workshop.Id == request.WorkshopId,
                cancellationToken);

        if (!workshopExists)
            return null;

        var sinceUtc = DateTime.UtcNow.AddDays(-RecentDays);

        var recentPlans = context.CuttingPlans
            .AsNoTracking()
            .Where(plan =>
                plan.WorkshopId == request.WorkshopId
                && plan.CreatedAtUtc >= sinceUtc);

        // KPIs reflect confirmed work only — drafts inflate waste without stocking remnants.
        var approvedPlans = recentPlans
            .Where(plan => plan.Status == CuttingPlanStatus.Approved);

        var totals = await approvedPlans
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalPieceLengthMm = group.Sum(plan => plan.TotalPieceLengthMm),
                TotalStockLengthMm = group.Sum(plan => plan.TotalStockLengthMm),
                TotalKerfMm = group.Sum(plan => plan.TotalKerfMm),
                TotalLeftoverMm = group.Sum(plan => plan.TotalLeftoverMm),
                StockBarsUsed = group.Sum(plan => plan.TotalBarsUsed),
                TotalMaterialCost = group.Sum(plan => plan.TotalMaterialCost)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var totalStock = totals?.TotalStockLengthMm ?? 0;
        var totalPieces = totals?.TotalPieceLengthMm ?? 0;
        var totalKerf = totals?.TotalKerfMm ?? 0;
        var totalLeftover = totals?.TotalLeftoverMm ?? 0;

        var efficiency = totalStock > 0
            ? (double)totalPieces / totalStock * 100d
            : 0d;

        var wastePercent = totalStock > 0
            ? (double)(totalKerf + totalLeftover) / totalStock * 100d
            : 0d;

        var kpis = new WorkshopDashboardKpisDto(
            Round1(efficiency),
            Round1(wastePercent),
            totals?.StockBarsUsed ?? 0,
            totals?.TotalMaterialCost ?? 0m);

        var latestPlans = await recentPlans
            .Where(plan => plan.ProjectId != null)
            .OrderByDescending(plan => plan.CreatedAtUtc)
            .Select(plan => new
            {
                plan.Id,
                ProjectId = plan.ProjectId!.Value,
                plan.CreatedAtUtc,
                plan.IsComplete,
                plan.TotalPieceLengthMm,
                plan.TotalStockLengthMm,
                MaterialName = plan.Material.Name,
                ProjectName = plan.Project!.Name
            })
            .ToListAsync(cancellationToken);

        var recentProjects = latestPlans
            .GroupBy(plan => plan.ProjectId)
            .Select(group => group.First())
            .Take(RecentProjectsLimit)
            .Select(plan =>
            {
                var projectEfficiency = plan.TotalStockLengthMm > 0
                    ? (double)plan.TotalPieceLengthMm / plan.TotalStockLengthMm * 100d
                    : 0d;

                return new DashboardRecentProjectDto(
                    plan.ProjectId,
                    plan.ProjectName,
                    plan.Id,
                    plan.MaterialName,
                    Round1(projectEfficiency),
                    plan.IsComplete,
                    plan.CreatedAtUtc);
            })
            .ToList();

        var workshopMaterialIds = context.CuttingPlans
            .AsNoTracking()
            .Where(plan => plan.WorkshopId == request.WorkshopId)
            .Select(plan => plan.MaterialId)
            .Distinct();

        var remnantInventory = await context.StockBars
            .AsNoTracking()
            .Where(stockBar =>
                stockBar.IsRemnant
                && stockBar.IsActive
                && stockBar.AvailableQuantity > 0
                && workshopMaterialIds.Contains(stockBar.MaterialId))
            .OrderByDescending(stockBar => stockBar.LengthMm)
            .ThenBy(stockBar => stockBar.Material.Name)
            .Select(stockBar => new RemnantInventoryItemDto(
                stockBar.Id,
                stockBar.MaterialId,
                stockBar.Material.Name,
                stockBar.LengthMm,
                stockBar.AvailableQuantity))
            .ToListAsync(cancellationToken);

        return new WorkshopDashboardDto(
            kpis,
            recentProjects,
            remnantInventory);
    }

    private static double Round1(double value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero);
}
