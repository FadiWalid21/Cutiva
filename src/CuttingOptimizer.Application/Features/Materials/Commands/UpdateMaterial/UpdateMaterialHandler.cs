using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Materials.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Materials.Commands.UpdateMaterial;

public sealed class UpdateMaterialHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateMaterialCommand, UpdateMaterialResult>
{
    public async Task<UpdateMaterialResult> Handle(
        UpdateMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var material = await context.Materials
            .Include(m => m.Sector)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material is null)
            return new UpdateMaterialNotFound();

        material.Name = request.Name.Trim();
        material.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        material.Category = request.Category;
        material.ThicknessMm = request.ThicknessMm;
        material.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateMaterialSucceeded(
            new MaterialDto(
                material.Id,
                material.SectorId,
                material.Sector.Code,
                material.Sector.Name,
                material.Code,
                material.Name,
                material.Description,
                material.Category,
                material.ThicknessMm,
                material.IsActive));
    }
}
