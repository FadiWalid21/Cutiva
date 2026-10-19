using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Queries.GetProductVariantById;

public sealed class GetProductVariantByIdHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductVariantByIdQuery, ProductVariantDto?>
{
    public async Task<ProductVariantDto?> Handle(
        GetProductVariantByIdQuery request,
        CancellationToken cancellationToken)
    {
        var variant = await context.ProductVariants
            .AsNoTracking()
            .Include(v => v.ProductType)
            .Include(v => v.Parts)
            .Include(v => v.GlassSlots)
            .Include(v => v.AccessorySlots)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        return variant is null
            ? null
            : ProductTemplateMapper.ToVariantDto(variant);
    }
}
