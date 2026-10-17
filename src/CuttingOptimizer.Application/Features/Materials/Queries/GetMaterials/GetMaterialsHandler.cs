using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Materials.Queries.GetMaterials;

public sealed class GetMaterialsHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetMaterialsQuery, IReadOnlyList<MaterialDto>>
{
    public async Task<IReadOnlyList<MaterialDto>> Handle(
        GetMaterialsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Materials
            .AsNoTracking()
            .AsQueryable();

        if (request.SectorId.HasValue)
        {
            query = query.Where(
                material => material.SectorId == request.SectorId.Value);
        }

        return await query
            .OrderBy(material => material.Name)
            .Select(material => new MaterialDto(
                material.Id,
                material.SectorId,
                material.Sector.Code,
                material.Sector.Name,
                material.Code,
                material.Name,
                material.Description))
            .ToListAsync(cancellationToken);
    }
}