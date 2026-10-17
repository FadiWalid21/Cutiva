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

        dbContext.Projects.Remove(project);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}