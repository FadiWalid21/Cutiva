using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Features.ProjectItems;

internal static class ProjectItemMapper
{
    public static (int WidthMm, int HeightMm) ResolveEffectiveDimensions(
        ProjectItem item)
    {
        var inspection = item.Inspection;
        if (inspection is
            {
                Status: InspectionStatus.Done,
                MeasuredWidthMm: not null,
                MeasuredHeightMm: not null
            })
        {
            return (
                inspection.MeasuredWidthMm.Value,
                inspection.MeasuredHeightMm.Value);
        }

        return (item.WidthMm, item.HeightMm);
    }

    public static ProjectItemDto ToDto(ProjectItem item)
    {
        var (effectiveW, effectiveH) = ResolveEffectiveDimensions(item);

        return new ProjectItemDto(
            item.Id,
            item.ProjectId,
            item.ProductVariantId,
            item.ProductVariant.Name,
            item.ProductVariant.Code,
            item.Label,
            item.WidthMm,
            item.HeightMm,
            item.Quantity,
            effectiveW,
            effectiveH,
            item.CalculationSnapshotJson,
            item.LastCalculatedAtUtc,
            item.Inspection is null ? null : ToInspectionDto(item.Inspection),
            item.PartChoices.Select(ToPartChoiceDto).ToList(),
            item.GlassChoices.Select(ToGlassChoiceDto).ToList(),
            item.AccessoryChoices.Select(ToAccessoryChoiceDto).ToList());
    }

    public static ProjectItemSummaryDto ToSummaryDto(ProjectItem item)
    {
        var (effectiveW, effectiveH) = ResolveEffectiveDimensions(item);
        return new ProjectItemSummaryDto(
            item.Id,
            item.ProductVariantId,
            item.ProductVariant.Name,
            item.Label,
            item.WidthMm,
            item.HeightMm,
            item.Quantity,
            effectiveW,
            effectiveH,
            item.LastCalculatedAtUtc);
    }

    public static ProjectItemInspectionDto ToInspectionDto(
        ProjectItemInspection inspection) =>
        new(
            inspection.Id,
            inspection.LocationLabel,
            inspection.MeasuredWidthMm,
            inspection.MeasuredHeightMm,
            inspection.InspectedAtUtc,
            inspection.InspectedBy,
            inspection.Notes,
            inspection.Status);

    public static ProjectItemPartChoiceDto ToPartChoiceDto(
        ProjectItemPartChoice choice) =>
        new(
            choice.Id,
            choice.TemplatePartId,
            choice.Role,
            choice.MaterialId,
            choice.Material.Code,
            choice.Material.Name);

    public static ProjectItemGlassChoiceDto ToGlassChoiceDto(
        ProjectItemGlassChoice choice) =>
        new(
            choice.Id,
            choice.TemplateGlassSlotId,
            choice.TemplateGlassSlot.Name,
            choice.GlassSpecId,
            choice.GlassSpec.ThicknessMm,
            choice.GlassSpec.PricePerSqm);

    public static ProjectItemAccessoryChoiceDto ToAccessoryChoiceDto(
        ProjectItemAccessoryChoice choice) =>
        new(
            choice.Id,
            choice.TemplateAccessorySlotId,
            choice.TemplateAccessorySlot.Name,
            choice.AccessoryId,
            choice.Accessory.Name,
            choice.Accessory.UnitPrice,
            choice.QuantityOverride);
}
