using CuttingOptimizer.Application.Features.Glass;
using MediatR;

namespace CuttingOptimizer.Application.Features.Glass.Queries.GetGlassSpecById;

public sealed record GetGlassSpecByIdQuery(Guid Id)
    : IRequest<GlassSpecDto?>;
