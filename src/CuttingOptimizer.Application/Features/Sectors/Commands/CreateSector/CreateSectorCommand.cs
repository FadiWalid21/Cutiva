using MediatR;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.CreateSector;

public sealed record CreateSectorCommand(
    string Code,
    string Name,
    Guid? MaterialId) : IRequest<CreateSectorOutcome>;

public enum CreateSectorStatus
{
    Created,
    CodeAlreadyExists,
    MaterialNotFound
}

public sealed record CreateSectorOutcome(
    CreateSectorStatus Status,
    Guid? Id = null);
