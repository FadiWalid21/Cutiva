using MediatR;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.CreateSector;

public sealed record CreateSectorCommand(
    string Code,
    string Name) : IRequest<Guid?>;