using CuttingOptimizer.Application.Features.ProjectItems;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.UpdateProjectItem;

public sealed record UpdateProjectItemCommand(
    Guid Id,
    string Label,
    int WidthMm,
    int HeightMm,
    int Quantity,
    IReadOnlyList<ProjectItemPartChoiceInput> PartChoices,
    IReadOnlyList<ProjectItemGlassChoiceInput> GlassChoices,
    IReadOnlyList<ProjectItemAccessoryChoiceInput> AccessoryChoices)
    : IRequest<UpdateProjectItemResult>;

public abstract record UpdateProjectItemResult;

public sealed record UpdateProjectItemSucceeded(ProjectItemDto Item)
    : UpdateProjectItemResult;

public sealed record UpdateProjectItemNotFound : UpdateProjectItemResult;

public sealed record UpdateProjectItemValidationFailed(string Code)
    : UpdateProjectItemResult;
