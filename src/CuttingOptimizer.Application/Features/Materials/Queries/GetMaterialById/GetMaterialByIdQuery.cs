using MediatR;

namespace CuttingOptimizer.Application.Features.Materials.Queries.GetMaterialById;

public sealed record GetMaterialByIdQuery(
    Guid Id
) : IRequest<MaterialDto?>;