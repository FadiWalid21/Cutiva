using CuttingOptimizer.Application.Features.Glass;
using MediatR;

namespace CuttingOptimizer.Application.Features.Glass.Commands.UpdateGlassType;

public sealed record UpdateGlassTypeCommand(
    Guid Id,
    string Name,
    bool IsActive = true)
    : IRequest<UpdateGlassTypeResult>;

public abstract record UpdateGlassTypeResult;

public sealed record UpdateGlassTypeSucceeded(GlassTypeDto GlassType)
    : UpdateGlassTypeResult;

public sealed record UpdateGlassTypeNotFound : UpdateGlassTypeResult;

public sealed record UpdateGlassTypeDuplicateName : UpdateGlassTypeResult;
