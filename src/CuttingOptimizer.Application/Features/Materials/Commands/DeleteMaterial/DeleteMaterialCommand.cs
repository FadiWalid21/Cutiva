using MediatR;

namespace CuttingOptimizer.Application.Features.Materials.Commands.DeleteMaterial;

public sealed record DeleteMaterialCommand(Guid Id) : IRequest<DeleteMaterialResult>;

public abstract record DeleteMaterialResult;

public sealed record DeleteMaterialSucceeded : DeleteMaterialResult;

public sealed record DeleteMaterialNotFound : DeleteMaterialResult;

public sealed record DeleteMaterialInUse : DeleteMaterialResult;
