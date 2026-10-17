using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.CreateRequiredCut;

public sealed class CreateRequiredCutHandler(
    IApplicationDbContext context)
    : IRequestHandler<CreateRequiredCutCommand, Guid?>
{
    public async Task<Guid?> Handle(
        CreateRequiredCutCommand request,
        CancellationToken cancellationToken)
    {
        var section = await context.ProjectSections
            .AsNoTracking()
            .Where(x => x.Id == request.ProjectSectionId)
            .Select(x => new { x.Id, x.SectorId })
            .FirstOrDefaultAsync(cancellationToken);

        if (section is null)
            return null;

        var materialMatchesSector = await context.Materials
            .AnyAsync(
                x => x.Id == request.MaterialId
                     && x.SectorId == section.SectorId,
                cancellationToken);

        if (!materialMatchesSector)
            return null;

        var requiredCut = new RequiredCut
        {
            ProjectSectionId = request.ProjectSectionId,
            MaterialId = request.MaterialId,
            Name = request.Name.Trim(),
            LengthMm = request.LengthMm,
            Quantity = request.Quantity
        };

        context.RequiredCuts.Add(requiredCut);

        await context.SaveChangesAsync(cancellationToken);

        return requiredCut.Id;
    }
}