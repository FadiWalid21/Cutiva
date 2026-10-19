using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProjectItems;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectItems.Queries.ListProjectItemsByProject;

public sealed class ListProjectItemsByProjectHandler(IApplicationDbContext context)
    : IRequestHandler<ListProjectItemsByProjectQuery, IReadOnlyList<ProjectItemSummaryDto>?>
{
    public async Task<IReadOnlyList<ProjectItemSummaryDto>?> Handle(
        ListProjectItemsByProjectQuery request,
        CancellationToken cancellationToken)
    {
        var projectExists = await context.Projects
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (!projectExists)
            return null;

        var items = await context.ProjectItems
            .AsNoTracking()
            .Include(i => i.ProductVariant)
            .Include(i => i.Inspection)
            .Where(i => i.ProjectId == request.ProjectId)
            .OrderBy(i => i.Label)
            .ToListAsync(cancellationToken);

        return items.Select(ProjectItemMapper.ToSummaryDto).ToList();
    }
}
