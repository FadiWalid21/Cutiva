using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Glass;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Glass.Queries.ListWorkshopGlassTypes;

public sealed class ListWorkshopGlassTypesHandler(
    IApplicationDbContext context)
    : IRequestHandler<ListWorkshopGlassTypesQuery, IReadOnlyList<GlassTypeDto>?>
{
    public async Task<IReadOnlyList<GlassTypeDto>?> Handle(
        ListWorkshopGlassTypesQuery request,
        CancellationToken cancellationToken)
    {
        var exists = await context.Workshops
            .AsNoTracking()
            .AnyAsync(w => w.Id == request.WorkshopId, cancellationToken);

        if (!exists)
            return null;

        var typesQuery = context.GlassTypes
            .AsNoTracking()
            .Where(t => t.WorkshopId == request.WorkshopId);

        if (request.ActiveOnly)
            typesQuery = typesQuery.Where(t => t.IsActive);

        return await typesQuery
            .OrderBy(t => t.Name)
            .Select(t => new GlassTypeDto(
                t.Id,
                t.WorkshopId,
                t.Name,
                t.IsActive,
                t.Specs
                    .Where(s => !request.ActiveOnly || s.IsActive)
                    .OrderBy(s => s.ThicknessMm)
                    .Select(s => new GlassSpecDto(
                        s.Id,
                        s.GlassTypeId,
                        s.ThicknessMm,
                        s.Description,
                        s.PricePerSqm,
                        s.IsActive))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
