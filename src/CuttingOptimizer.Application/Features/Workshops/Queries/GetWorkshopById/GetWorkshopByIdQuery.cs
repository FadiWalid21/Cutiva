using MediatR;

namespace CuttingOptimizer.Application.Features.Workshops.Queries.GetWorkshopById;

public sealed record GetWorkshopByIdQuery(Guid Id)
    : IRequest<WorkshopDto?>;

public sealed record WorkshopDto(
    Guid Id,
    string Name);