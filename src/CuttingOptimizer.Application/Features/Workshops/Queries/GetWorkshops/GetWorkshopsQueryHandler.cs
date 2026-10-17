using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Workshops.Queries.GetWorkshops;

public sealed class GetWorkshopsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetWorkshopsQuery, PagedWorkshopsDto>
{
    public async Task<PagedWorkshopsDto> Handle(
        GetWorkshopsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = dbContext.Workshops
            .AsNoTracking()
            .OrderBy(workshop => workshop.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(workshop => new WorkshopListItemDto(
                workshop.Id,
                workshop.Name))
            .ToListAsync(cancellationToken);

        return new PagedWorkshopsDto(
            items,
            page,
            pageSize,
            totalCount);
    }
}