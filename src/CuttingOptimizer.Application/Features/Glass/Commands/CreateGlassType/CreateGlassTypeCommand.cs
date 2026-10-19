using CuttingOptimizer.Application.Features.Glass;
using MediatR;

namespace CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassType;

public sealed record CreateGlassTypeCommand(
    Guid WorkshopId,
    string Name)
    : IRequest<CreateGlassTypeResult>;

public abstract record CreateGlassTypeResult;

public sealed record CreateGlassTypeSucceeded(GlassTypeDto GlassType)
    : CreateGlassTypeResult;

public sealed record CreateGlassTypeWorkshopNotFound : CreateGlassTypeResult;

public sealed record CreateGlassTypeDuplicateName : CreateGlassTypeResult;
