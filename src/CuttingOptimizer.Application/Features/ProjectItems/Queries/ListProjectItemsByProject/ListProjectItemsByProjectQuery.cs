using CuttingOptimizer.Application.Features.ProjectItems;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectItems.Queries.ListProjectItemsByProject;

public sealed record ListProjectItemsByProjectQuery(Guid ProjectId)
    : IRequest<IReadOnlyList<ProjectItemSummaryDto>?>;
