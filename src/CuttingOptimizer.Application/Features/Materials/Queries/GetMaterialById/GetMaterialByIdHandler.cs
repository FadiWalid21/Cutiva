using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Materials.Queries.GetMaterialById;

public sealed class GetMaterialByIdHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetMaterialByIdQuery, MaterialDto?>
{
    public async Task<MaterialDto?> Handle(
        GetMaterialByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Materials
            .AsNoTracking()
            .Where(material => material.Id == request.Id)
            .Select(material => new MaterialDto(
                material.Id,
                material.SectorId,
                material.Sector.Code,
                material.Sector.Name,
                material.Code,
                material.Name,
                material.Description,
                material.Category,
                material.ThicknessMm,
                material.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
