using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;

public sealed class CreateProjectSectionCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateProjectSectionCommand, Guid?>
{
    public async Task<Guid?> Handle(
        CreateProjectSectionCommand request,
        CancellationToken cancellationToken)
    {
        var projectExists = await dbContext.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
            return null;

        var sectorExists = await dbContext.Sectors
            .AnyAsync(
                sector => sector.Id == request.SectorId,
                cancellationToken);

        if (!sectorExists)
            return null;

        var section = new ProjectSection
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            SectorId = request.SectorId,
            Name = request.Name.Trim()
        };

        dbContext.ProjectSections.Add(section);

        await dbContext.SaveChangesAsync(cancellationToken);

        return section.Id;
    }
}