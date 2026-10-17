using MediatR;

namespace CuttingOptimizer.Application.Features.Projects.Commands.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid WorkshopId,
    Guid ProjectId,
    string Name,
    string? CustomerName) : IRequest<bool>;