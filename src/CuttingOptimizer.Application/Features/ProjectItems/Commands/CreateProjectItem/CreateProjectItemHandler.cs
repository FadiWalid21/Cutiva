using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProjectItems;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.CreateProjectItem;

public sealed class CreateProjectItemHandler(IApplicationDbContext context)
    : IRequestHandler<CreateProjectItemCommand, CreateProjectItemResult>
{
    public async Task<CreateProjectItemResult> Handle(
        CreateProjectItemCommand request,
        CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
            return new CreateProjectItemProjectNotFound();

        var variant = await context.ProductVariants
            .Include(v => v.Parts)
            .Include(v => v.GlassSlots)
            .Include(v => v.AccessorySlots)
            .FirstOrDefaultAsync(
                v => v.Id == request.ProductVariantId && v.IsActive,
                cancellationToken);

        if (variant is null)
            return new CreateProjectItemVariantNotFound();

        var validation = await ProjectItemChoiceValidator.ValidateAsync(
            context,
            project.WorkshopId,
            variant,
            request.PartChoices,
            request.GlassChoices,
            request.AccessoryChoices,
            cancellationToken);

        if (validation is ProjectItemChoiceValidator.Fail fail)
            return new CreateProjectItemValidationFailed(fail.Code);

        var item = new ProjectItem
        {
            ProjectId = request.ProjectId,
            ProductVariantId = request.ProductVariantId,
            Label = request.Label.Trim(),
            WidthMm = request.WidthMm,
            HeightMm = request.HeightMm,
            Quantity = request.Quantity
        };

        ProjectItemChoiceValidator.ApplyChoices(
            item,
            variant,
            request.PartChoices,
            request.GlassChoices,
            request.AccessoryChoices);
        ProjectItemChoiceValidator.ApplyInspection(item, request.Inspection);

        context.ProjectItems.Add(item);
        await context.SaveChangesAsync(cancellationToken);

        var loaded = await LoadForDtoAsync(context, item.Id, cancellationToken);
        return new CreateProjectItemSucceeded(ProjectItemMapper.ToDto(loaded!));
    }

    internal static Task<ProjectItem?> LoadForDtoAsync(
        IApplicationDbContext context,
        Guid id,
        CancellationToken cancellationToken) =>
        context.ProjectItems
            .AsNoTracking()
            .Include(i => i.ProductVariant)
            .Include(i => i.Inspection)
            .Include(i => i.PartChoices)
                .ThenInclude(c => c.Material)
            .Include(i => i.GlassChoices)
                .ThenInclude(c => c.TemplateGlassSlot)
            .Include(i => i.GlassChoices)
                .ThenInclude(c => c.GlassSpec)
            .Include(i => i.AccessoryChoices)
                .ThenInclude(c => c.TemplateAccessorySlot)
            .Include(i => i.AccessoryChoices)
                .ThenInclude(c => c.Accessory)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
}
