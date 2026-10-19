using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;

public sealed record CreateMaterialCommand(
    Guid SectorId,
    string Code,
    string Name,
    string? Description,
    MaterialCategory Category = MaterialCategory.Aluminum,
    decimal? ThicknessMm = null) : IRequest<Guid?>;
