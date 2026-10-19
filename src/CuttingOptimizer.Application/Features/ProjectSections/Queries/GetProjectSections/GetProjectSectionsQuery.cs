using CuttingOptimizer.Application.Features.Accessories;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProjectSections.Queries.GetProjectSections;

public sealed record GetProjectSectionsQuery(Guid ProjectId)
    : IRequest<IReadOnlyList<ProjectSectionDto>?>;

public sealed record ProjectSectionDto(
    Guid Id,
    Guid ProjectId,
    Guid SectorId,
    string SectorCode,
    string SectorName,
    string Name,
    int? HeightMm,
    int? WidthMm,
    int? UnitQuantity,
    IReadOnlyList<SectionAccessoryDto> Accessories);
