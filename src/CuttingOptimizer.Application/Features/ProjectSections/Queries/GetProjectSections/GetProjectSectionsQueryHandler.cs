using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectSections.Queries.GetProjectSections;

public sealed class GetProjectSectionsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetProjectSectionsQuery, IReadOnlyList<ProjectSectionDto>?>
{
    public async Task<IReadOnlyList<ProjectSectionDto>?> Handle(
        GetProjectSectionsQuery request,
        CancellationToken cancellationToken)
    {
        var projectExists = await dbContext.Projects
            .AnyAsync(
                project => project.Id == request.ProjectId,
                cancellationToken);

        if (!projectExists)
            return null;

        return await dbContext.ProjectSections
            .AsNoTracking()
            .Where(section => section.ProjectId == request.ProjectId)
            .OrderBy(section => section.Name)
            .Select(section => new ProjectSectionDto(
                section.Id,
                section.ProjectId,
                section.SectorId,
                section.Sector.Code,
                section.Sector.Name,
                section.Name))
            .ToListAsync(cancellationToken);
    }
}