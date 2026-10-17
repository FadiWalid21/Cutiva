using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<UpdateProjectCommand, bool>
{
    public async Task<bool> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .FirstOrDefaultAsync(
                project => project.Id == request.ProjectId
                           && project.WorkshopId == request.WorkshopId,
                cancellationToken);

        if (project is null)
            return false;

        project.Name = request.Name.Trim();
        project.CustomerName = string.IsNullOrWhiteSpace(request.CustomerName)
            ? null
            : request.CustomerName.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}