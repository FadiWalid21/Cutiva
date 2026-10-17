using MediatR;

namespace CuttingOptimizer.Application.Features.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery(
    Guid WorkshopId,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedProjectsDto?>;

public sealed record PagedProjectsDto(
    IReadOnlyList<ProjectListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record ProjectListItemDto(
    Guid Id,
    Guid WorkshopId,
    string Name,
    string? CustomerName);