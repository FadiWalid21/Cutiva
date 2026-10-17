using MediatR;

namespace CuttingOptimizer.Application.Features.Sectors.Queries.GetSectors;

public sealed record GetSectorsQuery()
    : IRequest<IReadOnlyList<SectorDto>>;

public sealed record SectorDto(
    Guid Id,
    string Code,
    string Name);