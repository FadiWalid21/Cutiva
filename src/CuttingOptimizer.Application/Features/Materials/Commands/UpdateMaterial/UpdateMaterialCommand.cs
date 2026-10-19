using CuttingOptimizer.Application.Features.Materials.Queries;
using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.Materials.Commands.UpdateMaterial;

public sealed record UpdateMaterialCommand(
    Guid Id,
    string Name,
    string? Description,
    MaterialCategory Category,
    decimal? ThicknessMm,
    bool IsActive) : IRequest<UpdateMaterialResult>;

public abstract record UpdateMaterialResult;

public sealed record UpdateMaterialSucceeded(MaterialDto Material)
    : UpdateMaterialResult;

public sealed record UpdateMaterialNotFound : UpdateMaterialResult;
