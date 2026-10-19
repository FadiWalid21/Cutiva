using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Features.ProjectItems;

public sealed record ProjectItemPartChoiceInput(
    Guid TemplatePartId,
    Guid MaterialId);

public sealed record ProjectItemGlassChoiceInput(
    Guid TemplateGlassSlotId,
    Guid GlassSpecId);

public sealed record ProjectItemAccessoryChoiceInput(
    Guid TemplateAccessorySlotId,
    Guid AccessoryId,
    int? QuantityOverride = null);

public sealed record ProjectItemInspectionInput(
    string? LocationLabel,
    int? MeasuredWidthMm,
    int? MeasuredHeightMm,
    DateTime? InspectedAtUtc,
    string? InspectedBy,
    string? Notes,
    InspectionStatus Status);

public sealed record ProjectItemPartChoiceDto(
    Guid Id,
    Guid TemplatePartId,
    TemplatePartRole Role,
    Guid MaterialId,
    string MaterialCode,
    string MaterialName);

public sealed record ProjectItemGlassChoiceDto(
    Guid Id,
    Guid TemplateGlassSlotId,
    string SlotName,
    Guid GlassSpecId,
    decimal ThicknessMm,
    decimal PricePerSqm);

public sealed record ProjectItemAccessoryChoiceDto(
    Guid Id,
    Guid TemplateAccessorySlotId,
    string SlotName,
    Guid AccessoryId,
    string AccessoryName,
    decimal UnitPrice,
    int? QuantityOverride);

public sealed record ProjectItemInspectionDto(
    Guid Id,
    string? LocationLabel,
    int? MeasuredWidthMm,
    int? MeasuredHeightMm,
    DateTime? InspectedAtUtc,
    string? InspectedBy,
    string? Notes,
    InspectionStatus Status);

public sealed record ProjectItemDto(
    Guid Id,
    Guid ProjectId,
    Guid ProductVariantId,
    string ProductVariantName,
    string ProductVariantCode,
    string Label,
    int WidthMm,
    int HeightMm,
    int Quantity,
    int EffectiveWidthMm,
    int EffectiveHeightMm,
    string? CalculationSnapshotJson,
    DateTime? LastCalculatedAtUtc,
    ProjectItemInspectionDto? Inspection,
    IReadOnlyList<ProjectItemPartChoiceDto> PartChoices,
    IReadOnlyList<ProjectItemGlassChoiceDto> GlassChoices,
    IReadOnlyList<ProjectItemAccessoryChoiceDto> AccessoryChoices);

public sealed record ProjectItemSummaryDto(
    Guid Id,
    Guid ProductVariantId,
    string ProductVariantName,
    string Label,
    int WidthMm,
    int HeightMm,
    int Quantity,
    int EffectiveWidthMm,
    int EffectiveHeightMm,
    DateTime? LastCalculatedAtUtc);
