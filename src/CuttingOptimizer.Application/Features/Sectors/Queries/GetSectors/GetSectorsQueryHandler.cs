using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Sectors.Queries.GetSectors;

public sealed class GetSectorsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetSectorsQuery, IReadOnlyList<SectorDto>>
{
    public async Task<IReadOnlyList<SectorDto>> Handle(
        GetSectorsQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Sectors
            .AsNoTracking()
            .OrderBy(sector => sector.Name)
            .Select(sector => new SectorDto(
                sector.Id,
                sector.Code,
                sector.Name))
            .ToListAsync(cancellationToken);
    }
}