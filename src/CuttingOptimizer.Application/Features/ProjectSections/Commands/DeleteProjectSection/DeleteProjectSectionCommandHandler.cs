using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectSections.Commands.DeleteProjectSection;

public sealed class DeleteProjectSectionCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteProjectSectionCommand, DeleteProjectSectionResult>
{
    public async Task<DeleteProjectSectionResult> Handle(
        DeleteProjectSectionCommand request,
        CancellationToken cancellationToken)
    {
        var section = await dbContext.ProjectSections
            .FirstOrDefaultAsync(
                s => s.Id == request.SectionId && s.ProjectId == request.ProjectId,
                cancellationToken);

        if (section is null)
            return DeleteProjectSectionResult.NotFound;

        var cutIds = await dbContext.RequiredCuts
            .Where(c => c.ProjectSectionId == section.Id)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (cutIds.Count > 0)
        {
            var usedInPlan = await dbContext.CuttingPlanPieces
                .AsNoTracking()
                .AnyAsync(p => cutIds.Contains(p.RequiredCutId), cancellationToken)
                || await dbContext.CuttingPlanUnallocatedPieces
                    .AsNoTracking()
                    .AnyAsync(p => cutIds.Contains(p.RequiredCutId), cancellationToken);

            if (usedInPlan)
                return DeleteProjectSectionResult.InUse;

            var cuts = await dbContext.RequiredCuts
                .Where(c => c.ProjectSectionId == section.Id)
                .ToListAsync(cancellationToken);

            dbContext.RequiredCuts.RemoveRange(cuts);
        }

        dbContext.ProjectSections.Remove(section);
        await dbContext.SaveChangesAsync(cancellationToken);

        return DeleteProjectSectionResult.Deleted;
    }
}
