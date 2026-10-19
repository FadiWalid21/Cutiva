using System.ComponentModel.DataAnnotations;
using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Api.Contracts.ProjectItems;

public sealed record ProjectItemPartChoiceRequest(
    Guid TemplatePartId,
    Guid MaterialId);

public sealed record ProjectItemGlassChoiceRequest(
    Guid TemplateGlassSlotId,
    Guid GlassSpecId);

public sealed record ProjectItemAccessoryChoiceRequest(
    Guid TemplateAccessorySlotId,
    Guid AccessoryId,
    int? QuantityOverride = null);

public sealed record ProjectItemInspectionRequest(
    string? LocationLabel,
    int? MeasuredWidthMm,
    int? MeasuredHeightMm,
    DateTime? InspectedAtUtc,
    string? InspectedBy,
    string? Notes,
    InspectionStatus Status = InspectionStatus.Pending);

public sealed record CreateProjectItemRequest(
    [Required]
    Guid ProductVariantId,

    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Label,

    [Range(1, int.MaxValue)]
    int WidthMm,

    [Range(1, int.MaxValue)]
    int HeightMm,

    [Range(1, int.MaxValue)]
    int Quantity,

    IReadOnlyList<ProjectItemPartChoiceRequest> PartChoices,

    IReadOnlyList<ProjectItemGlassChoiceRequest> GlassChoices,

    IReadOnlyList<ProjectItemAccessoryChoiceRequest> AccessoryChoices,

    ProjectItemInspectionRequest? Inspection = null);

public sealed record UpdateProjectItemRequest(
    [Required]
    [StringLength(200, MinimumLength = 1)]
    string Label,

    [Range(1, int.MaxValue)]
    int WidthMm,

    [Range(1, int.MaxValue)]
    int HeightMm,

    [Range(1, int.MaxValue)]
    int Quantity,

    IReadOnlyList<ProjectItemPartChoiceRequest> PartChoices,

    IReadOnlyList<ProjectItemGlassChoiceRequest> GlassChoices,

    IReadOnlyList<ProjectItemAccessoryChoiceRequest> AccessoryChoices);

public sealed record UpsertProjectItemInspectionRequest(
    string? LocationLabel,
    int? MeasuredWidthMm,
    int? MeasuredHeightMm,
    DateTime? InspectedAtUtc,
    string? InspectedBy,
    string? Notes,
    InspectionStatus Status = InspectionStatus.Pending);

public sealed record CalculateProjectRequest(
    [Range(0, int.MaxValue)]
    int KerfMm = 3,

    bool UseRemnants = true);
