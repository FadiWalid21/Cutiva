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

        if (await IsInUseAsync(request.Id, cancellationToken))
            return new DeleteMaterialInUse();

        var linkedSectors = await context.Sectors
            .Where(s => s.MaterialId == material.Id)
            .ToListAsync(cancellationToken);
        foreach (var sector in linkedSectors)
            sector.MaterialId = null;

        var stockBars = await context.StockBars
            .Where(b => b.MaterialId == material.Id)
            .ToListAsync(cancellationToken);
        context.StockBars.RemoveRange(stockBars);

        context.Materials.Remove(material);
        await context.SaveChangesAsync(cancellationToken);
        return new DeleteMaterialSucceeded();
    }

    private async Task<bool> IsInUseAsync(Guid materialId, CancellationToken cancellationToken)
    {
        if (await context.RequiredCuts.AnyAsync(c => c.MaterialId == materialId, cancellationToken))
            return true;

        if (await context.CuttingPlans.AnyAsync(p => p.MaterialId == materialId, cancellationToken))
            return true;

        if (await context.TemplatePartAllowedMaterials.AnyAsync(
                a => a.MaterialId == materialId,
                cancellationToken))
            return true;

        if (await context.TemplateParts.AnyAsync(
                t => t.DefaultAluminumMaterialId == materialId,
                cancellationToken))
            return true;

        if (await context.ProjectItemPartChoices.AnyAsync(
                c => c.MaterialId == materialId,
                cancellationToken))
            return true;

        var stockBarIds = await context.StockBars
            .Where(b => b.MaterialId == materialId)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        if (stockBarIds.Count == 0)
            return false;

        return await context.CuttingPlanBars.AnyAsync(
            bar => stockBarIds.Contains(bar.StockBarId),
            cancellationToken);
    }
}
