using CuttingOptimizer.Application.Features.ProjectItems;
using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.UpsertProjectItemInspection;

public sealed record UpsertProjectItemInspectionCommand(
    Guid ProjectItemId,
    string? LocationLabel,
    int? MeasuredWidthMm,
    int? MeasuredHeightMm,
    DateTime? InspectedAtUtc,
    string? InspectedBy,
    string? Notes,
    InspectionStatus Status)
    : IRequest<UpsertProjectItemInspectionResult>;

public abstract record UpsertProjectItemInspectionResult;

public sealed record UpsertProjectItemInspectionSucceeded(ProjectItemInspectionDto Inspection)
    : UpsertProjectItemInspectionResult;

public sealed record UpsertProjectItemInspectionNotFound : UpsertProjectItemInspectionResult;
