using MediatR;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.UpdateSector;

public sealed record UpdateSectorCommand(
    Guid Id,
    string Code,
    string Name) : IRequest<UpdateSectorResult>;

public enum UpdateSectorResult
{
    Updated,
    NotFound,
    CodeAlreadyExists
}