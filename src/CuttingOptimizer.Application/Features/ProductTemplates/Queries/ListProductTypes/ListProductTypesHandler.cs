using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProductTemplates;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Queries.ListProductTypes;

public sealed class ListProductTypesHandler(IApplicationDbContext context)
    : IRequestHandler<ListProductTypesQuery, IReadOnlyList<ProductTypeDto>>
{
    public async Task<IReadOnlyList<ProductTypeDto>> Handle(
        ListProductTypesQuery request,
        CancellationToken cancellationToken)
    {
        IQueryable<ProductType> query = context.ProductTypes
            .AsNoTracking()
            .Include(t => t.Variants)
                .ThenInclude(v => v.Parts)
            .Include(t => t.Variants)
                .ThenInclude(v => v.GlassSlots)
            .Include(t => t.Variants)
                .ThenInclude(v => v.AccessorySlots);

        if (request.ActiveOnly)
            query = query.Where(t => t.IsActive);

        var types = await query
            .OrderBy(t => t.Kind)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

        if (request.ActiveOnly)
        {
            foreach (var type in types)
            {
                type.Variants = type.Variants
                    .Where(v => v.IsActive)
                    .ToList();
            }
        }

        return types.Select(ProductTemplateMapper.ToTypeDto).ToList();
    }
}
