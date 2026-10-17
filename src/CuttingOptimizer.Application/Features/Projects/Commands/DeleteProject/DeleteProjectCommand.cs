using MediatR;

namespace CuttingOptimizer.Application.Features.Projects.Commands.DeleteProject;

public sealed record DeleteProjectCommand(
    Guid WorkshopId,
    Guid ProjectId) : IRequest<bool>;