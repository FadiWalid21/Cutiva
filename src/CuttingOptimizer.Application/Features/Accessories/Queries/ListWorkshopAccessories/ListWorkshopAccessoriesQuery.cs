using CuttingOptimizer.Application.Features.Accessories;
using MediatR;

namespace CuttingOptimizer.Application.Features.Accessories.Queries.ListWorkshopAccessories;

public sealed record ListWorkshopAccessoriesQuery(
    Guid WorkshopId,
    bool ActiveOnly = true)
    : IRequest<IReadOnlyList<AccessoryDto>?>;
