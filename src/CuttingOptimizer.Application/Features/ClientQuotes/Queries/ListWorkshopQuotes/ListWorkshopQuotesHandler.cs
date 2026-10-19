using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ClientQuotes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ClientQuotes.Queries.ListWorkshopQuotes;

public sealed class ListWorkshopQuotesHandler(
    IApplicationDbContext context)
    : IRequestHandler<ListWorkshopQuotesQuery, IReadOnlyList<ClientQuoteSummaryDto>?>
{
    public async Task<IReadOnlyList<ClientQuoteSummaryDto>?> Handle(
        ListWorkshopQuotesQuery request,
        CancellationToken cancellationToken)
    {
        var workshop = await context.Workshops
            .AsNoTracking()
            .Where(w => w.Id == request.WorkshopId)
            .Select(w => new { w.Id, w.ProductionPricePerM2 })
            .FirstOrDefaultAsync(cancellationToken);

        if (workshop is null)
            return null;

        var quotes = await context.ClientQuotes
            .AsNoTracking()
            .Where(q => q.Project.WorkshopId == request.WorkshopId)
            .OrderByDescending(q => q.CreatedAtUtc)
            .Select(q => new
            {
                q.Id,
                q.ProjectId,
                ProjectName = q.Project.Name,
                q.Project.CustomerName,
                q.CuttingPlanId,
                MaterialName = q.CuttingPlan != null
                    ? q.CuttingPlan.Material.Name
                    : string.Empty,
                MaterialCost = q.CuttingPlan != null
                    ? q.CuttingPlan.TotalMaterialCost
                    : 0m,
                SurchargesTotal = q.Surcharges.Sum(s => (decimal?)s.Amount) ?? 0m,
                q.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var projectIds = quotes.Select(q => q.ProjectId).Distinct().ToList();

        var accessoriesByProject = await context.ProjectSectionAccessories
            .AsNoTracking()
            .Where(a => projectIds.Contains(a.ProjectSection.ProjectId))
            .GroupBy(a => a.ProjectSection.ProjectId)
            .Select(g => new
            {
                ProjectId = g.Key,
                Total = g.Sum(x => x.Accessory.UnitPrice * x.Quantity)
            })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Total, cancellationToken);

        var sectionRows = await context.ProjectSections
            .AsNoTracking()
            .Where(s => projectIds.Contains(s.ProjectId))
            .Select(s => new
            {
                s.ProjectId,
                Section = new QuoteSectionPreviewDto(
                    s.Id,
                    s.Name,
                    s.Sector.Code,
                    s.Sector.Name,
                    s.HeightMm,
                    s.WidthMm,
                    s.UnitQuantity,
                    s.Sector.MaterialId != null)
            })
            .ToListAsync(cancellationToken);

        var sectionsMap = sectionRows
            .GroupBy(x => x.ProjectId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<QuoteSectionPreviewDto>)g.Select(x => x.Section).ToList());

        return quotes
            .Select(q =>
            {
                accessoriesByProject.TryGetValue(q.ProjectId, out var accessories);
                sectionsMap.TryGetValue(q.ProjectId, out var sections);
                var production = ClientQuoteMapper.ComputeProductionCost(
                    sections ?? [],
                    workshop.ProductionPricePerM2);
                var materialLabel = string.IsNullOrEmpty(q.MaterialName)
                    ? "Accessories"
                    : q.MaterialName;
                return new ClientQuoteSummaryDto(
                    q.Id,
                    q.ProjectId,
                    q.ProjectName,
                    q.CustomerName,
                    q.CuttingPlanId,
                    materialLabel,
                    q.MaterialCost + production + accessories + q.SurchargesTotal,
                    q.CreatedAtUtc);
            })
            .ToList();
    }
}
