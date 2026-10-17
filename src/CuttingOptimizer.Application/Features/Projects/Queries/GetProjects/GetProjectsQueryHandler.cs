using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetProjectsQuery, PagedProjectsDto?>
{
    public async Task<PagedProjectsDto?> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var workshopExists = await dbContext.Workshops
            .AnyAsync(
                workshop => workshop.Id == request.WorkshopId,
                cancellationToken);

        if (!workshopExists)
            return null;
        
        var query = dbContext.Projects
            .AsNoTracking()
            .Where(project => project.WorkshopId == request.WorkshopId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(project => project.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(project => new ProjectListItemDto(
                project.Id,
                project.WorkshopId,
                project.Name,
                project.CustomerName))
            .ToListAsync(cancellationToken);

        return new PagedProjectsDto(
            items,
            page,
            pageSize,
            totalCount);
    }
}