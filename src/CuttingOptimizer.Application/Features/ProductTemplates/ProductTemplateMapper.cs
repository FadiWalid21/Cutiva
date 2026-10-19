using CuttingOptimizer.Domain.Entities;

namespace CuttingOptimizer.Application.Features.ProductTemplates;

internal static class ProductTemplateMapper
{
    public static ProductTypeDto ToTypeDto(ProductType type) =>
        new(
            type.Id,
            type.Kind,
            type.Name,
            type.IsActive,
            type.IsSeeded,
            type.Variants
                .OrderBy(v => v.Name)
                .Select(ToVariantSummaryDto)
                .ToList());

    public static ProductVariantSummaryDto ToVariantSummaryDto(ProductVariant variant) =>
        new(
            variant.Id,
            variant.Name,
            variant.Code,
            variant.SashCount,
            variant.OverlapMm,
            variant.IsActive,
            variant.IsSeeded,
            variant.Parts.Count,
            variant.GlassSlots.Count,
            variant.AccessorySlots.Count);

    public static ProductVariantDto ToVariantDto(ProductVariant variant) =>
        new(
            variant.Id,
            variant.ProductTypeId,
            variant.ProductType.Kind,
            variant.ProductType.Name,
            variant.Name,
            variant.Code,
            variant.SashCount,
            variant.OverlapMm,
            variant.IsActive,
            variant.IsSeeded,
            variant.Parts.OrderBy(p => p.Role).Select(ToPartDto).ToList(),
            variant.GlassSlots.OrderBy(s => s.Name).Select(ToGlassSlotDto).ToList(),
            variant.AccessorySlots.OrderBy(s => s.Name).Select(ToAccessorySlotDto).ToList());

    public static TemplatePartDto ToPartDto(TemplatePart part) =>
        new(
            part.Id,
            part.ProductVariantId,
            part.Role,
            part.DisplayName,
            part.QtyPerUnit,
            part.WidthCoef,
            part.HeightCoef,
            part.OffsetMm,
            part.UseSashWidthDivision,
            part.DefaultAluminumMaterialId);

    public static TemplateGlassSlotDto ToGlassSlotDto(TemplateGlassSlot slot) =>
        new(
            slot.Id,
            slot.ProductVariantId,
            slot.Name,
            slot.PaneCount,
            slot.WidthCoefW,
            slot.HeightCoefW,
            slot.OffsetWMm,
            slot.WidthCoefH,
            slot.HeightCoefH,
            slot.OffsetHMm,
            slot.Required,
            slot.UseSashWidthDivision);

    public static TemplateAccessorySlotDto ToAccessorySlotDto(TemplateAccessorySlot slot) =>
        new(
            slot.Id,
            slot.ProductVariantId,
            slot.Name,
            slot.QuantityRule,
            slot.Qty,
            slot.DefaultAccessoryId);
}
