using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.UpdateSector;

public sealed class UpdateSectorCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateSectorCommand, UpdateSectorResult>
{
    public async Task<UpdateSectorResult> Handle(
        UpdateSectorCommand request,
        CancellationToken cancellationToken)
    {
        var sector = await dbContext.Sectors
            .FirstOrDefaultAsync(
                sector => sector.Id == request.Id,
                cancellationToken);

        if (sector is null)
            return UpdateSectorResult.NotFound;

        var code = request.Code.Trim().ToUpperInvariant();

        var codeExists = await dbContext.Sectors
            .AnyAsync(
                existing => existing.Code == code
                            && existing.Id != request.Id,
                cancellationToken);

        if (codeExists)
            return UpdateSectorResult.CodeAlreadyExists;

        if (request.MaterialId.HasValue)
        {
            var materialOk = await dbContext.Materials
                .AsNoTracking()
                .AnyAsync(
                    material => material.Id == request.MaterialId.Value,
                    cancellationToken);

            if (!materialOk)
                return UpdateSectorResult.MaterialNotFound;
        }

        sector.Code = code;
        sector.Name = request.Name.Trim();
        sector.MaterialId = request.MaterialId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return UpdateSectorResult.Updated;
    }
}
