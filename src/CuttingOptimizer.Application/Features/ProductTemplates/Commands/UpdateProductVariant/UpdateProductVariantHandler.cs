using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateProductVariantCommand, UpdateProductVariantResult>
{
    public async Task<UpdateProductVariantResult> Handle(
        UpdateProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var variant = await context.ProductVariants
            .Include(v => v.ProductType)
            .Include(v => v.Parts)
            .Include(v => v.GlassSlots)
            .Include(v => v.AccessorySlots)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (variant is null)
            return new UpdateProductVariantNotFound();

        variant.Name = request.Name.Trim();
        variant.SashCount = request.SashCount;
        variant.OverlapMm = request.OverlapMm;
        variant.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateProductVariantSucceeded(
            ProductTemplateMapper.ToVariantDto(variant));
    }
}
