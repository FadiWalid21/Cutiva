using CuttingOptimizer.Application.Features.Accessories;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;

public sealed record CreateProjectSectionCommand(
    Guid ProjectId,
    Guid SectorId,
    string Name,
    int HeightMm,
    int WidthMm,
    int UnitQuantity,
    IReadOnlyList<SectionAccessoryInput>? Accessories)
    : IRequest<CreateProjectSectionResult>;

public abstract record CreateProjectSectionResult;

public sealed record CreateProjectSectionSucceeded(Guid SectionId)
    : CreateProjectSectionResult;

public sealed record CreateProjectSectionNotFound : CreateProjectSectionResult;

public sealed record CreateProjectSectionInvalidAccessories : CreateProjectSectionResult;
