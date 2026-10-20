using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;

public sealed record CreateMaterialCommand(
    Guid SectorId,
    string Code,
    string Name,
    string? Description,
    MaterialCategory Category = MaterialCategory.Aluminum,
    decimal? ThicknessMm = null) : IRequest<CreateMaterialResult>;

public abstract record CreateMaterialResult;

public sealed record CreateMaterialSucceeded(Guid Id, string Code, bool Reactivated) : CreateMaterialResult;

public sealed record CreateMaterialSectorNotFound : CreateMaterialResult;

public sealed record CreateMaterialCodeExists(string Code) : CreateMaterialResult;
