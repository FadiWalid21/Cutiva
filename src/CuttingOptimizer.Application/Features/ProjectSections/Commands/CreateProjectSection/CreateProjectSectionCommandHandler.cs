using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;

public sealed class CreateProjectSectionCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateProjectSectionCommand, CreateProjectSectionResult>
{
    public async Task<CreateProjectSectionResult> Handle(
        CreateProjectSectionCommand request,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == request.ProjectId)
            .Select(p => new { p.Id, p.WorkshopId })
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
            return new CreateProjectSectionNotFound();

        var sectorExists = await dbContext.Sectors
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.SectorId, cancellationToken);

        if (!sectorExists)
            return new CreateProjectSectionNotFound();

        var accessoryInputs = (request.Accessories ?? [])
            .Where(a => a.Quantity > 0)
            .GroupBy(a => a.AccessoryId)
            .Select(g => new { AccessoryId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        if (accessoryInputs.Count > 0)
        {
            var ids = accessoryInputs.Select(a => a.AccessoryId).ToList();
            var validCount = await dbContext.Accessories
                .CountAsync(
                    a => ids.Contains(a.Id)
                         && a.WorkshopId == project.WorkshopId
                         && a.IsActive,
                    cancellationToken);

            if (validCount != ids.Count)
                return new CreateProjectSectionInvalidAccessories();
        }

        var section = new ProjectSection
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            SectorId = request.SectorId,
            Name = request.Name.Trim(),
            HeightMm = request.HeightMm,
            WidthMm = request.WidthMm,
            UnitQuantity = request.UnitQuantity
        };

        dbContext.ProjectSections.Add(section);

        foreach (var line in accessoryInputs)
        {
            dbContext.ProjectSectionAccessories.Add(new ProjectSectionAccessory
            {
                ProjectSectionId = section.Id,
                AccessoryId = line.AccessoryId,
                Quantity = line.Quantity
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectSectionSucceeded(section.Id);
    }
}
