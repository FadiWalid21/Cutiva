using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Sectors.Commands.CreateSector;

public sealed class CreateSectorCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateSectorCommand, Guid?>
{
    public async Task<Guid?> Handle(
        CreateSectorCommand request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();

        var codeExists = await dbContext.Sectors
            .AnyAsync(
                sector => sector.Code == code,
                cancellationToken);

        if (codeExists)
            return null;

        var sector = new Sector
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim()
        };

        dbContext.Sectors.Add(sector);

        await dbContext.SaveChangesAsync(cancellationToken);

        return sector.Id;
    }
}