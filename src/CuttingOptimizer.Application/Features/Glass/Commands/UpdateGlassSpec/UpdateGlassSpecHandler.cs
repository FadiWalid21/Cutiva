using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassType;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Glass.Commands.UpdateGlassSpec;

public sealed class UpdateGlassSpecHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateGlassSpecCommand, UpdateGlassSpecResult>
{
    public async Task<UpdateGlassSpecResult> Handle(
        UpdateGlassSpecCommand request,
        CancellationToken cancellationToken)
    {
        var spec = await context.GlassSpecs
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (spec is null)
            return new UpdateGlassSpecNotFound();

        spec.ThicknessMm = request.ThicknessMm;
        spec.PricePerSqm = request.PricePerSqm;
        spec.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        spec.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateGlassSpecSucceeded(
            CreateGlassTypeHandler.ToSpecDto(spec));
    }
}
