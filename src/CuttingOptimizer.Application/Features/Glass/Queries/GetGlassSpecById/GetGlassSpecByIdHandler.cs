using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Glass;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Glass.Queries.GetGlassSpecById;

public sealed class GetGlassSpecByIdHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetGlassSpecByIdQuery, GlassSpecDto?>
{
    public async Task<GlassSpecDto?> Handle(
        GetGlassSpecByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.GlassSpecs
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new GlassSpecDto(
                s.Id,
                s.GlassTypeId,
                s.ThicknessMm,
                s.Description,
                s.PricePerSqm,
                s.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
