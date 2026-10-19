using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Glass;
using CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassType;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassSpec;

public sealed class CreateGlassSpecHandler(
    IApplicationDbContext context)
    : IRequestHandler<CreateGlassSpecCommand, CreateGlassSpecResult>
{
    public async Task<CreateGlassSpecResult> Handle(
        CreateGlassSpecCommand request,
        CancellationToken cancellationToken)
    {
        var typeExists = await context.GlassTypes
            .AnyAsync(t => t.Id == request.GlassTypeId, cancellationToken);

        if (!typeExists)
            return new CreateGlassSpecTypeNotFound();

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var spec = new GlassSpec
        {
            GlassTypeId = request.GlassTypeId,
            ThicknessMm = request.ThicknessMm,
            Description = description,
            PricePerSqm = request.PricePerSqm,
            IsActive = true
        };

        context.GlassSpecs.Add(spec);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateGlassSpecSucceeded(
            CreateGlassTypeHandler.ToSpecDto(spec));
    }
}
