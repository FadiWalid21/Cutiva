using MediatR;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.UpdateWorkshop;

public sealed record UpdateWorkshopCommand(
    Guid Id,
    string Name) : IRequest<bool>;