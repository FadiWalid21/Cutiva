using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Queries.GetProductVariantById;

public sealed record GetProductVariantByIdQuery(Guid Id)
    : IRequest<ProductVariantDto?>;
