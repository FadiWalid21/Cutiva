using MediatR;

namespace CuttingOptimizer.Application.Features.Materials.Queries.GetMaterials;

public sealed record GetMaterialsQuery(
    Guid? SectorId = null
) : IRequest<IReadOnlyList<MaterialDto>>;