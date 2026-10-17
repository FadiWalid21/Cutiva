using MediatR;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.DeleteWorkshop;

public sealed record DeleteWorkshopCommand(Guid Id) : IRequest<bool>;
