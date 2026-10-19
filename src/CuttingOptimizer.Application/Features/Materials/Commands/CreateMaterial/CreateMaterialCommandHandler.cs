using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;

public sealed class CreateMaterialCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateMaterialCommand, Guid?>
{
    public async Task<Guid?> Handle(
        CreateMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var sectorExists = await dbContext.Sectors
            .AnyAsync(
                sector => sector.Id == request.SectorId,
                cancellationToken);

        if (!sectorExists)
            return null;

        var code = request.Code.Trim().ToUpperInvariant();

        var codeExists = await dbContext.Materials
            .AnyAsync(
                material => material.SectorId == request.SectorId
                            && material.Code == code,
                cancellationToken);

        if (codeExists)
            return null;

        var material = new Material
        {
            Id = Guid.NewGuid(),
            SectorId = request.SectorId,
            Code = code,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Category = request.Category,
            ThicknessMm = request.ThicknessMm,
            IsActive = true
        };

        dbContext.Materials.Add(material);

        await dbContext.SaveChangesAsync(cancellationToken);

        return material.Id;
    }
}
