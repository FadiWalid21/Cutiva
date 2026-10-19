using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.DeleteProjectItem;

public sealed record DeleteProjectItemCommand(Guid Id)
    : IRequest<bool>;
