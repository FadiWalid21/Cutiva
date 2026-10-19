using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProjectItems;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.UpsertProjectItemInspection;

public sealed class UpsertProjectItemInspectionHandler(IApplicationDbContext context)
    : IRequestHandler<UpsertProjectItemInspectionCommand, UpsertProjectItemInspectionResult>
{
    public async Task<UpsertProjectItemInspectionResult> Handle(
        UpsertProjectItemInspectionCommand request,
        CancellationToken cancellationToken)
    {
        var item = await context.ProjectItems
            .Include(i => i.Inspection)
            .FirstOrDefaultAsync(i => i.Id == request.ProjectItemId, cancellationToken);

        if (item is null)
            return new UpsertProjectItemInspectionNotFound();

        ProjectItemChoiceValidator.ApplyInspection(
            item,
            new ProjectItemInspectionInput(
                request.LocationLabel,
                request.MeasuredWidthMm,
                request.MeasuredHeightMm,
                request.InspectedAtUtc,
                request.InspectedBy,
                request.Notes,
                request.Status));

        item.Inspection ??= new ProjectItemInspection { ProjectItem = item };

        await context.SaveChangesAsync(cancellationToken);

        return new UpsertProjectItemInspectionSucceeded(
            ProjectItemMapper.ToInspectionDto(item.Inspection!));
    }
}
