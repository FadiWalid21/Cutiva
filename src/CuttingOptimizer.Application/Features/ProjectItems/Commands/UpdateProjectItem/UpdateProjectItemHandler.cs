using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProjectItems;
using CuttingOptimizer.Application.Features.ProjectItems.Commands.CreateProjectItem;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.UpdateProjectItem;

public sealed class UpdateProjectItemHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateProjectItemCommand, UpdateProjectItemResult>
{
    public async Task<UpdateProjectItemResult> Handle(
        UpdateProjectItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await context.ProjectItems
            .Include(i => i.Project)
            .Include(i => i.PartChoices)
            .Include(i => i.GlassChoices)
            .Include(i => i.AccessoryChoices)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (item is null)
            return new UpdateProjectItemNotFound();

        var variant = await context.ProductVariants
            .Include(v => v.Parts)
            .Include(v => v.GlassSlots)
            .Include(v => v.AccessorySlots)
            .FirstOrDefaultAsync(
                v => v.Id == item.ProductVariantId,
                cancellationToken);

        if (variant is null)
            return new UpdateProjectItemNotFound();

        var validation = await ProjectItemChoiceValidator.ValidateAsync(
            context,
            item.Project.WorkshopId,
            variant,
            request.PartChoices,
            request.GlassChoices,
            request.AccessoryChoices,
            cancellationToken);

        if (validation is ProjectItemChoiceValidator.Fail fail)
            return new UpdateProjectItemValidationFailed(fail.Code);

        item.Label = request.Label.Trim();
        item.WidthMm = request.WidthMm;
        item.HeightMm = request.HeightMm;
        item.Quantity = request.Quantity;

        ProjectItemChoiceValidator.ApplyChoices(
            item,
            variant,
            request.PartChoices,
            request.GlassChoices,
            request.AccessoryChoices);

        await context.SaveChangesAsync(cancellationToken);

        var loaded = await CreateProjectItemHandler.LoadForDtoAsync(
            context,
            item.Id,
            cancellationToken);

        return new UpdateProjectItemSucceeded(ProjectItemMapper.ToDto(loaded!));
    }
}
