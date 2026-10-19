using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.CreateSector;

public sealed class CreateSectorCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateSectorCommand, CreateSectorOutcome>
{
    public async Task<CreateSectorOutcome> Handle(
        CreateSectorCommand request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        var codeExists = await dbContext.Sectors
            .AnyAsync(
                sector => sector.Code == code,
                cancellationToken);

        if (codeExists)
            return new CreateSectorOutcome(CreateSectorStatus.CodeAlreadyExists);

        if (request.MaterialId.HasValue)
        {
            var materialOk = await dbContext.Materials
                .AsNoTracking()
                .AnyAsync(
                    material => material.Id == request.MaterialId.Value,
                    cancellationToken);

            if (!materialOk)
                return new CreateSectorOutcome(CreateSectorStatus.MaterialNotFound);
        }

        var sector = new Sector
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            MaterialId = request.MaterialId
        };

        dbContext.Sectors.Add(sector);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateSectorOutcome(CreateSectorStatus.Created, sector.Id);
    }
}
