using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProjectItems;
using CuttingOptimizer.Application.Features.ProjectItems.Commands.CreateProjectItem;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.DuplicateProjectItem;

public sealed class DuplicateProjectItemHandler(IApplicationDbContext context)
    : IRequestHandler<DuplicateProjectItemCommand, DuplicateProjectItemResult>
{
    public async Task<DuplicateProjectItemResult> Handle(
        DuplicateProjectItemCommand request,
        CancellationToken cancellationToken)
    {
        var source = await context.ProjectItems
            .AsNoTracking()
            .Include(i => i.Inspection)
            .Include(i => i.PartChoices)
            .Include(i => i.GlassChoices)
            .Include(i => i.AccessoryChoices)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (source is null)
            return new DuplicateProjectItemNotFound();

        var clone = new ProjectItem
        {
            ProjectId = source.ProjectId,
            ProductVariantId = source.ProductVariantId,
            Label = $"{source.Label} (copy)",
            WidthMm = source.WidthMm,
            HeightMm = source.HeightMm,
            Quantity = source.Quantity
        };

        foreach (var choice in source.PartChoices)
        {
            clone.PartChoices.Add(new ProjectItemPartChoice
            {
                TemplatePartId = choice.TemplatePartId,
                Role = choice.Role,
                MaterialId = choice.MaterialId
            });
        }

        foreach (var choice in source.GlassChoices)
        {
            clone.GlassChoices.Add(new ProjectItemGlassChoice
            {
                TemplateGlassSlotId = choice.TemplateGlassSlotId,
                GlassSpecId = choice.GlassSpecId
            });
        }

        foreach (var choice in source.AccessoryChoices)
        {
            clone.AccessoryChoices.Add(new ProjectItemAccessoryChoice
            {
                TemplateAccessorySlotId = choice.TemplateAccessorySlotId,
                AccessoryId = choice.AccessoryId,
                QuantityOverride = choice.QuantityOverride
            });
        }

        if (source.Inspection is not null)
        {
            clone.Inspection = new ProjectItemInspection
            {
                LocationLabel = source.Inspection.LocationLabel,
                MeasuredWidthMm = source.Inspection.MeasuredWidthMm,
                MeasuredHeightMm = source.Inspection.MeasuredHeightMm,
                InspectedAtUtc = source.Inspection.InspectedAtUtc,
                InspectedBy = source.Inspection.InspectedBy,
                Notes = source.Inspection.Notes,
                Status = source.Inspection.Status
            };
        }

        context.ProjectItems.Add(clone);
        await context.SaveChangesAsync(cancellationToken);

        var loaded = await CreateProjectItemHandler.LoadForDtoAsync(
            context,
            clone.Id,
            cancellationToken);

        return new DuplicateProjectItemSucceeded(ProjectItemMapper.ToDto(loaded!));
    }
}
