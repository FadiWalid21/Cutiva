using CuttingOptimizer.Application.Features.ProjectItems;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.CreateProjectItem;

public sealed record CreateProjectItemCommand(
    Guid ProjectId,
    Guid ProductVariantId,
    string Label,
    int WidthMm,
    int HeightMm,
    int Quantity,
    IReadOnlyList<ProjectItemPartChoiceInput> PartChoices,
    IReadOnlyList<ProjectItemGlassChoiceInput> GlassChoices,
    IReadOnlyList<ProjectItemAccessoryChoiceInput> AccessoryChoices,
    ProjectItemInspectionInput? Inspection = null)
    : IRequest<CreateProjectItemResult>;

public abstract record CreateProjectItemResult;

public sealed record CreateProjectItemSucceeded(ProjectItemDto Item)
    : CreateProjectItemResult;

public sealed record CreateProjectItemProjectNotFound : CreateProjectItemResult;

public sealed record CreateProjectItemVariantNotFound : CreateProjectItemResult;

public sealed record CreateProjectItemValidationFailed(string Code)
    : CreateProjectItemResult;
