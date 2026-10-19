using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.Materials.Queries.GetMaterials;

public sealed record GetMaterialsQuery(
    Guid? SectorId = null,
    MaterialCategory? Category = null,
    bool? ActiveOnly = null)
    : IRequest<IReadOnlyList<MaterialDto>>;
