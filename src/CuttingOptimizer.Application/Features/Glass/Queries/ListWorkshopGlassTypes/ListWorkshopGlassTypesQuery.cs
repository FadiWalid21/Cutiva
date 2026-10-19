using CuttingOptimizer.Application.Features.Glass;
using MediatR;

namespace CuttingOptimizer.Application.Features.Glass.Queries.ListWorkshopGlassTypes;

public sealed record ListWorkshopGlassTypesQuery(
    Guid WorkshopId,
    bool ActiveOnly = true)
    : IRequest<IReadOnlyList<GlassTypeDto>?>;
