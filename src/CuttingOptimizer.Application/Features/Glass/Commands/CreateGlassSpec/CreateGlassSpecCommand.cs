using CuttingOptimizer.Application.Features.Glass;
using MediatR;

namespace CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassSpec;

public sealed record CreateGlassSpecCommand(
    Guid GlassTypeId,
    decimal ThicknessMm,
    decimal PricePerSqm,
    string? Description = null)
    : IRequest<CreateGlassSpecResult>;

public abstract record CreateGlassSpecResult;

public sealed record CreateGlassSpecSucceeded(GlassSpecDto Spec)
    : CreateGlassSpecResult;

public sealed record CreateGlassSpecTypeNotFound : CreateGlassSpecResult;
