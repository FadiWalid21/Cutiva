using MediatR;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.UpdateWorkshop;

public sealed record UpdateWorkshopCommand(
    Guid Id,
    string Name,
    decimal? ProductionPricePerM2 = null) : IRequest<bool>;
