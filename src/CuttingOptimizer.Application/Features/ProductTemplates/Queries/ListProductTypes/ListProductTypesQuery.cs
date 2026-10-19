using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Queries.ListProductTypes;

public sealed record ListProductTypesQuery(bool ActiveOnly = false)
    : IRequest<IReadOnlyList<ProductTypeDto>>;
