using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Reports.Queries.GetWorkshopReport;

public sealed class GetWorkshopReportHandler(IApplicationDbContext context)
    : IRequestHandler<GetWorkshopReportQuery, WorkshopReportDto?>
{
    public async Task<WorkshopReportDto?> Handle(
        GetWorkshopReportQuery request,
        CancellationToken cancellationToken)
    {
        var workshopExists = await context.Workshops
            .AsNoTracking()
            .AnyAsync(w => w.Id == request.WorkshopId, cancellationToken);

        if (!workshopExists)
            return null;

        var from = request.FromUtc;
        var to = request.ToUtc;
        if (to < from)
            (from, to) = (to, from);

        var plans = await context.CuttingPlans
            .AsNoTracking()
            .Where(p =>
                p.WorkshopId == request.WorkshopId
                && p.CreatedAtUtc >= from
                && p.CreatedAtUtc <= to)
            .Select(p => new
            {
                p.Id,
                p.ProjectId,
                ProjectName = p.Project != null ? p.Project.Name : null,
                p.MaterialId,
                MaterialName = p.Material.Name,
                p.Status,
                p.TotalBarsUsed,
                p.TotalPieceLengthMm,
                p.TotalStockLengthMm,
                p.TotalKerfMm,
                p.TotalLeftoverMm,
                p.TotalMaterialCost,
                p.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var plansGenerated = plans.Count;
        var approved = plans.Where(p => p.Status == CuttingPlanStatus.Approved).ToList();
        var plansApproved = approved.Count;
        var projectsActive = plans
            .Where(p => p.ProjectId.HasValue)
            .Select(p => p.ProjectId!.Value)
            .Distinct()
            .Count();

        var totalStock = approved.Sum(p => p.TotalStockLengthMm);
        var totalPieces = approved.Sum(p => p.TotalPieceLengthMm);
        var totalKerf = approved.Sum(p => p.TotalKerfMm);
        var totalLeftover = approved.Sum(p => p.TotalLeftoverMm);
        var stockBarsUsed = approved.Sum(p => p.TotalBarsUsed);
        var totalCost = approved.Sum(p => p.TotalMaterialCost);

        var efficiency = totalStock > 0
            ? (double)totalPieces / totalStock * 100d
            : 0d;
        var wastePercent = totalStock > 0
            ? (double)(totalKerf + totalLeftover) / totalStock * 100d
            : 0d;

        var workshopMaterialIds = context.CuttingPlans
            .AsNoTracking()
            .Where(p => p.WorkshopId == request.WorkshopId)
            .Select(p => p.MaterialId)
            .Distinct();

        var remnantInStock = await context.StockBars
            .AsNoTracking()
            .CountAsync(
                s => s.IsRemnant
                     && s.IsActive
                     && s.AvailableQuantity > 0
                     && workshopMaterialIds.Contains(s.MaterialId),
                cancellationToken);

        var remnantConsumed = await context.CuttingPlanBars
            .AsNoTracking()
            .Where(b =>
                b.CuttingPlan.WorkshopId == request.WorkshopId
                && b.CuttingPlan.Status == CuttingPlanStatus.Approved
                && b.CuttingPlan.CreatedAtUtc >= from
                && b.CuttingPlan.CreatedAtUtc <= to
                && b.StockBar.IsRemnant)
            .Select(b => b.StockBarId)
            .Distinct()
            .CountAsync(cancellationToken);

        var topMaterials = approved
            .GroupBy(p => new { p.MaterialId, p.MaterialName })
            .Select(g =>
            {
                var stock = g.Sum(x => x.TotalStockLengthMm);
                var waste = stock > 0
                    ? (double)(g.Sum(x => x.TotalKerfMm) + g.Sum(x => x.TotalLeftoverMm))
                      / stock * 100d
                    : 0d;
                return new WorkshopReportMaterialRowDto(
                    g.Key.MaterialId,
                    g.Key.MaterialName,
                    g.Count(),
                    g.Sum(x => x.TotalBarsUsed),
                    g.Sum(x => x.TotalMaterialCost),
                    Round1(waste));
            })
            .OrderByDescending(r => r.TotalMaterialCost)
            .Take(10)
            .ToList();

        var projectRows = plans
            .Where(p => p.ProjectId.HasValue)
            .GroupBy(p => new { ProjectId = p.ProjectId!.Value, Name = p.ProjectName ?? "—" })
            .Select(g => new WorkshopReportProjectRowDto(
                g.Key.ProjectId,
                g.Key.Name,
                g.Count(),
                g.Count(x => x.Status == CuttingPlanStatus.Approved),
                g.Where(x => x.Status == CuttingPlanStatus.Approved)
                    .Sum(x => x.TotalMaterialCost),
                g.Max(x => x.CreatedAtUtc)))
            .OrderByDescending(r => r.LastActivityUtc)
            .Take(50)
            .ToList();

        return new WorkshopReportDto(
            from,
            to,
            new WorkshopReportKpisDto(
                projectsActive,
                plansGenerated,
                plansApproved,
                stockBarsUsed,
                remnantInStock,
                remnantConsumed,
                Round1(efficiency),
                Round1(wastePercent),
                totalCost),
            topMaterials,
            projectRows);
    }

    private static double Round1(double value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero);
}
