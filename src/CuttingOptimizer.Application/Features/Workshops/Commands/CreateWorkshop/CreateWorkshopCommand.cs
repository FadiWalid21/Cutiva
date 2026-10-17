using MediatR;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;

public sealed record CreateWorkshopCommand(string Name)
    : IRequest<Guid>;