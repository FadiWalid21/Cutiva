using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Accessories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Accessories.Queries.ListWorkshopAccessories;

public sealed class ListWorkshopAccessoriesHandler(
    IApplicationDbContext context)
    : IRequestHandler<ListWorkshopAccessoriesQuery, IReadOnlyList<AccessoryDto>?>
{
    public async Task<IReadOnlyList<AccessoryDto>?> Handle(
        ListWorkshopAccessoriesQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Workshops
            .AsNoTracking()
            .AnyAsync(w => w.Id == request.WorkshopId, cancellationToken);

        if (!exists)
            return null;

        var query = context.Accessories
            .AsNoTracking()
            .Where(a => a.WorkshopId == request.WorkshopId);

        if (request.ActiveOnly)
            query = query.Where(a => a.IsActive);

        return await query
            .OrderBy(a => a.Name)
            .Select(a => new AccessoryDto(
                a.Id,
                a.WorkshopId,
                a.Name,
                a.Brand,
                a.Unit,
                a.UnitPrice,
                a.IsActive,
                a.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
