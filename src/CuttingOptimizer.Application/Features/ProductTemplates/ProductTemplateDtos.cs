using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Features.ProductTemplates;

public sealed record ProductTypeDto(
    Guid Id,
    ProductKind Kind,
    string Name,
    bool IsActive,
    bool IsSeeded,
    IReadOnlyList<ProductVariantSummaryDto> Variants);

public sealed record ProductVariantSummaryDto(
    Guid Id,
    string Name,
    string Code,
    int? SashCount,
    int? OverlapMm,
    bool IsActive,
    bool IsSeeded,
    int PartCount,
    int GlassSlotCount,
    int AccessorySlotCount);

public sealed record ProductVariantDto(
    Guid Id,
    Guid ProductTypeId,
    ProductKind ProductKind,
    string ProductTypeName,
    string Name,
    string Code,
    int? SashCount,
    int? OverlapMm,
    bool IsActive,
    bool IsSeeded,
    IReadOnlyList<TemplatePartDto> Parts,
    IReadOnlyList<TemplateGlassSlotDto> GlassSlots,
    IReadOnlyList<TemplateAccessorySlotDto> AccessorySlots);

public sealed record TemplatePartDto(
    Guid Id,
    Guid ProductVariantId,
    TemplatePartRole Role,
    string DisplayName,
    int QtyPerUnit,
    decimal WidthCoef,
    decimal HeightCoef,
    int OffsetMm,
    bool UseSashWidthDivision,
    Guid? DefaultAluminumMaterialId);

public sealed record TemplateGlassSlotDto(
    Guid Id,
    Guid ProductVariantId,
    string Name,
    int PaneCount,
    decimal WidthCoefW,
    decimal HeightCoefW,
    int OffsetWMm,
    decimal WidthCoefH,
    decimal HeightCoefH,
    int OffsetHMm,
    bool Required,
    bool UseSashWidthDivision);

public sealed record TemplateAccessorySlotDto(
    Guid Id,
    Guid ProductVariantId,
    string Name,
    AccessoryQuantityRule QuantityRule,
    int Qty,
    Guid? DefaultAccessoryId);
