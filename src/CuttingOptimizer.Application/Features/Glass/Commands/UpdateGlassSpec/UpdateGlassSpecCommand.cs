using CuttingOptimizer.Application.Features.Glass;
using MediatR;

namespace CuttingOptimizer.Application.Features.Glass.Commands.UpdateGlassSpec;

public sealed record UpdateGlassSpecCommand(
    Guid Id,
    decimal ThicknessMm,
    decimal PricePerSqm,
    string? Description = null,
    bool IsActive = true)
    : IRequest<UpdateGlassSpecResult>;

public abstract record UpdateGlassSpecResult;

public sealed record UpdateGlassSpecSucceeded(GlassSpecDto Spec)
    : UpdateGlassSpecResult;

public sealed record UpdateGlassSpecNotFound : UpdateGlassSpecResult;
