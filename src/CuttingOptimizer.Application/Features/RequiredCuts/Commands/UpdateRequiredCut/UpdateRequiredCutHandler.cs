using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.UpdateRequiredCut;

public sealed class UpdateRequiredCutHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateRequiredCutCommand, bool>
{
    public async Task<bool> Handle(
        UpdateRequiredCutCommand request,
        CancellationToken cancellationToken)
    {
        var requiredCut = await context.RequiredCuts
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (requiredCut is null)
            return false;

        var sectionSectorId = await context.ProjectSections
            .Where(x => x.Id == request.ProjectSectionId)
            .Select(x => (Guid?)x.SectorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (sectionSectorId is null)
            return false;

        var materialMatchesSector = await context.Materials
            .AnyAsync(
                x => x.Id == request.MaterialId
                     && x.SectorId == sectionSectorId.Value,
                cancellationToken);

        if (!materialMatchesSector)
            return false;

        requiredCut.ProjectSectionId = request.ProjectSectionId;
        requiredCut.MaterialId = request.MaterialId;
        requiredCut.Name = request.Name.Trim();
        requiredCut.LengthMm = request.LengthMm;
        requiredCut.Quantity = request.Quantity;

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}