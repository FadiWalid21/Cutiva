using CuttingOptimizer.Application.Features.ProjectItems;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.DuplicateProjectItem;

public sealed record DuplicateProjectItemCommand(Guid Id)
    : IRequest<DuplicateProjectItemResult>;

public abstract record DuplicateProjectItemResult;

public sealed record DuplicateProjectItemSucceeded(ProjectItemDto Item)
    : DuplicateProjectItemResult;

public sealed record DuplicateProjectItemNotFound : DuplicateProjectItemResult;
