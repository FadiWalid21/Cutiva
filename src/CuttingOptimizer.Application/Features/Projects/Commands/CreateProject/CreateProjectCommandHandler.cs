using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Projects;

public sealed class CreateProjectCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateProjectCommand, Guid?>
{
    public async Task<Guid?> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var workshopExists = await dbContext.Workshops
            .AnyAsync(
                workshop => workshop.Id == request.WorkshopId,
                cancellationToken);

        if (!workshopExists)
            return null;

        var project = new Project
        {
            Id = Guid.NewGuid(),
            WorkshopId = request.WorkshopId,
            Name = request.Name.Trim(),
            CustomerName = string.IsNullOrWhiteSpace(request.CustomerName)
                ? null
                : request.CustomerName.Trim()
        };

        dbContext.Projects.Add(project);

        await dbContext.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}