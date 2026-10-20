using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Materials.Commands.DeleteMaterial;

public sealed class DeleteMaterialHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteMaterialCommand, DeleteMaterialResult>
{
    public async Task<DeleteMaterialResult> Handle(
        DeleteMaterialCommand request,
        CancellationToken cancellationToken)
    {
        var material = await context.Materials
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (material is null)
            return new DeleteMaterialNotFound();

        if (!material.IsActive)
            return new DeleteMaterialSucceeded();

        material.IsActive = false;

        var stockBars = await context.StockBars
            .Where(b => b.MaterialId == material.Id && b.IsActive)
            .ToListAsync(cancellationToken);
        foreach (var bar in stockBars)
            bar.IsActive = false;

        var linkedSectors = await context.Sectors
            .Where(s => s.MaterialId == material.Id)
            .ToListAsync(cancellationToken);
        foreach (var sector in linkedSectors)
            sector.MaterialId = null;

        await context.SaveChangesAsync(cancellationToken);
        return new DeleteMaterialSucceeded();
    }
}
