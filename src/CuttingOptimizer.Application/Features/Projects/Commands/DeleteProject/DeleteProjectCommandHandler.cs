using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<DeleteProjectCommand, bool>
{
    public async Task<bool> Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .FirstOrDefaultAsync(
                project => project.Id == request.ProjectId
                           && project.WorkshopId == request.WorkshopId,
                cancellationToken);

        if (project is null)
            return false;

        var plans = await dbContext.CuttingPlans
            .Where(plan => plan.ProjectId == project.Id)
            .ToListAsync(cancellationToken);

        if (plans.Count > 0)
            dbContext.CuttingPlans.RemoveRange(plans);

        var sectionIds = await dbContext.ProjectSections
            .Where(section => section.ProjectId == project.Id)
            .Select(section => section.Id)
            .ToListAsync(cancellationToken);

        if (sectionIds.Count > 0)
        {
            var cuts = await dbContext.RequiredCuts
                .Where(cut => sectionIds.Contains(cut.ProjectSectionId))
                .ToListAsync(cancellationToken);

            if (cuts.Count > 0)
                dbContext.RequiredCuts.RemoveRange(cuts);

            var sections = await dbContext.ProjectSections
                .Where(section => section.ProjectId == project.Id)
                .ToListAsync(cancellationToken);

            dbContext.ProjectSections.RemoveRange(sections);
        }

        dbContext.Projects.Remove(project);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
