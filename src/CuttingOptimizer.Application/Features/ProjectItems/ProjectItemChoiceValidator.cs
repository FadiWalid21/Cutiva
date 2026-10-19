using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProjectItems;

internal static class ProjectItemChoiceValidator
{
    public abstract record ValidationResult;

    public sealed record Ok : ValidationResult;

    public sealed record Fail(string Code) : ValidationResult;

    public static async Task<ValidationResult> ValidateAsync(
        IApplicationDbContext context,
        Guid workshopId,
        ProductVariant variant,
        IReadOnlyList<ProjectItemPartChoiceInput> partChoices,
        IReadOnlyList<ProjectItemGlassChoiceInput> glassChoices,
        IReadOnlyList<ProjectItemAccessoryChoiceInput> accessoryChoices,
        CancellationToken cancellationToken)
    {
        var partIds = variant.Parts.Select(p => p.Id).ToHashSet();
        var glassSlotIds = variant.GlassSlots.Select(s => s.Id).ToHashSet();
        var accessorySlotIds = variant.AccessorySlots.Select(s => s.Id).ToHashSet();

        if (partChoices.Select(c => c.TemplatePartId).Distinct().Count()
            != partChoices.Count)
            return new Fail("DuplicatePartChoice");

        if (glassChoices.Select(c => c.TemplateGlassSlotId).Distinct().Count()
            != glassChoices.Count)
            return new Fail("DuplicateGlassChoice");

        if (accessoryChoices.Select(c => c.TemplateAccessorySlotId).Distinct().Count()
            != accessoryChoices.Count)
            return new Fail("DuplicateAccessoryChoice");

        foreach (var choice in partChoices)
        {
            if (!partIds.Contains(choice.TemplatePartId))
                return new Fail("TemplatePartNotInVariant");
        }

        foreach (var part in variant.Parts)
        {
            if (partChoices.All(c => c.TemplatePartId != part.Id))
                return new Fail("MissingPartChoice");
        }

        foreach (var choice in glassChoices)
        {
            if (!glassSlotIds.Contains(choice.TemplateGlassSlotId))
                return new Fail("GlassSlotNotInVariant");
        }

        foreach (var choice in accessoryChoices)
        {
            if (!accessorySlotIds.Contains(choice.TemplateAccessorySlotId))
                return new Fail("AccessorySlotNotInVariant");
        }

        foreach (var requiredSlot in variant.GlassSlots.Where(s => s.Required))
        {
            if (glassChoices.All(c => c.TemplateGlassSlotId != requiredSlot.Id))
                return new Fail("RequiredGlassSlotMissing");
        }

        var materialIds = partChoices
            .Select(c => c.MaterialId)
            .Distinct()
            .ToList();

        if (materialIds.Count > 0)
        {
            var validMaterialCount = await context.Materials
                .CountAsync(
                    m => materialIds.Contains(m.Id)
                         && m.IsActive
                         && m.Category == MaterialCategory.Aluminum,
                    cancellationToken);

            if (validMaterialCount != materialIds.Count)
                return new Fail("InvalidAluminumMaterial");
        }

        var glassSpecIds = glassChoices
            .Select(c => c.GlassSpecId)
            .Distinct()
            .ToList();

        if (glassSpecIds.Count > 0)
        {
            var validGlassCount = await context.GlassSpecs
                .CountAsync(
                    s => glassSpecIds.Contains(s.Id)
                         && s.IsActive
                         && s.GlassType.WorkshopId == workshopId,
                    cancellationToken);

            if (validGlassCount != glassSpecIds.Count)
                return new Fail("InvalidGlassSpec");
        }

        var accessoryIds = accessoryChoices
            .Select(c => c.AccessoryId)
            .Distinct()
            .ToList();

        if (accessoryIds.Count > 0)
        {
            var validAccessoryCount = await context.Accessories
                .CountAsync(
                    a => accessoryIds.Contains(a.Id)
                         && a.IsActive
                         && a.WorkshopId == workshopId,
                    cancellationToken);

            if (validAccessoryCount != accessoryIds.Count)
                return new Fail("InvalidAccessory");
        }

        return new Ok();
    }

    public static void ApplyChoices(
        ProjectItem item,
        ProductVariant variant,
        IReadOnlyList<ProjectItemPartChoiceInput> partChoices,
        IReadOnlyList<ProjectItemGlassChoiceInput> glassChoices,
        IReadOnlyList<ProjectItemAccessoryChoiceInput> accessoryChoices)
    {
        item.PartChoices.Clear();
        item.GlassChoices.Clear();
        item.AccessoryChoices.Clear();

        var partsById = variant.Parts.ToDictionary(p => p.Id);

        foreach (var choice in partChoices)
        {
            var part = partsById[choice.TemplatePartId];
            item.PartChoices.Add(new ProjectItemPartChoice
            {
                ProjectItem = item,
                TemplatePartId = choice.TemplatePartId,
                Role = part.Role,
                MaterialId = choice.MaterialId
            });
        }

        foreach (var choice in glassChoices)
        {
            item.GlassChoices.Add(new ProjectItemGlassChoice
            {
                ProjectItem = item,
                TemplateGlassSlotId = choice.TemplateGlassSlotId,
                GlassSpecId = choice.GlassSpecId
            });
        }

        foreach (var choice in accessoryChoices)
        {
            item.AccessoryChoices.Add(new ProjectItemAccessoryChoice
            {
                ProjectItem = item,
                TemplateAccessorySlotId = choice.TemplateAccessorySlotId,
                AccessoryId = choice.AccessoryId,
                QuantityOverride = choice.QuantityOverride
            });
        }
    }

    public static void ApplyInspection(
        ProjectItem item,
        ProjectItemInspectionInput? input)
    {
        if (input is null)
            return;

        item.Inspection ??= new ProjectItemInspection
        {
            ProjectItem = item
        };

        item.Inspection.LocationLabel = string.IsNullOrWhiteSpace(input.LocationLabel)
            ? null
            : input.LocationLabel.Trim();
        item.Inspection.MeasuredWidthMm = input.MeasuredWidthMm;
        item.Inspection.MeasuredHeightMm = input.MeasuredHeightMm;
        item.Inspection.InspectedAtUtc = input.InspectedAtUtc;
        item.Inspection.InspectedBy = string.IsNullOrWhiteSpace(input.InspectedBy)
            ? null
            : input.InspectedBy.Trim();
        item.Inspection.Notes = string.IsNullOrWhiteSpace(input.Notes)
            ? null
            : input.Notes.Trim();
        item.Inspection.Status = input.Status;
    }
}
