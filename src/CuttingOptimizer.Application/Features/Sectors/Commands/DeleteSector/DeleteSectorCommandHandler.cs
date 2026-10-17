using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.DeleteSector;

public sealed class DeleteSectorCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteSectorCommand, DeleteSectorResult>
{
    public async Task<DeleteSectorResult> Handle(
        DeleteSectorCommand request,
        CancellationToken cancellationToken)
    {
        var sector = await dbContext.Sectors
            .FirstOrDefaultAsync(
                sector => sector.Id == request.Id,
                cancellationToken);

        if (sector is null)
            return DeleteSectorResult.NotFound;

        var isInUse = await dbContext.ProjectSections
            .AnyAsync(
                section => section.SectorId == request.Id,
                cancellationToken);

        if (isInUse)
            return DeleteSectorResult.InUse;

        dbContext.Sectors.Remove(sector);

        await dbContext.SaveChangesAsync(cancellationToken);

        return DeleteSectorResult.Deleted;
    }
}