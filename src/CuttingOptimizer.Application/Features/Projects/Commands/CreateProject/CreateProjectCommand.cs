using MediatR;

namespace CuttingOptimizer.Application.Features.Projects;


public sealed record CreateProjectCommand(
    Guid WorkshopId,
    string Name,
    string? CustomerName) : IRequest<Guid?>;