using CuttingOptimizer.Application.Features.ProjectItems;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectItems.Queries.GetProjectItemById;

public sealed record GetProjectItemByIdQuery(Guid Id)
    : IRequest<ProjectItemDto?>;
