using MediatR;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.DeleteSector;

public sealed record DeleteSectorCommand(Guid Id)
    : IRequest<DeleteSectorResult>;

public enum DeleteSectorResult
{
    Deleted,
    NotFound,
    InUse
}