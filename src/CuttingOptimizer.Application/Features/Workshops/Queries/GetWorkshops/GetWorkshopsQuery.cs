using MediatR;

namespace CuttingOptimizer.Application.Features.Workshops.Queries.GetWorkshops;


public sealed record GetWorkshopsQuery(
    int Page = 1,
    int PageSize = 10)
    : IRequest<PagedWorkshopsDto>;

public sealed record PagedWorkshopsDto(
    IReadOnlyList<WorkshopListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record WorkshopListItemDto(
    Guid Id,
    string Name);
    
    