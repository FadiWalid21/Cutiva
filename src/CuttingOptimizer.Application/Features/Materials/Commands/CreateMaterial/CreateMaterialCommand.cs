using MediatR;

namespace CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;

public sealed record CreateMaterialCommand(
    Guid SectorId,
    string Code,
    string Name,
    string? Description) : IRequest<Guid?>;