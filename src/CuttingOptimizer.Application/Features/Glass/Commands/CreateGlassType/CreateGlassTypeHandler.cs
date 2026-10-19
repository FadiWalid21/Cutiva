using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Glass;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassType;

public sealed class CreateGlassTypeHandler(
    IApplicationDbContext context)
    : IRequestHandler<CreateGlassTypeCommand, CreateGlassTypeResult>
{
    public async Task<CreateGlassTypeResult> Handle(
        CreateGlassTypeCommand request,
        CancellationToken cancellationToken)
    {
        var workshopExists = await context.Workshops
            .AnyAsync(w => w.Id == request.WorkshopId, cancellationToken);

        if (!workshopExists)
            return new CreateGlassTypeWorkshopNotFound();

        var name = request.Name.Trim();
        var duplicate = await context.GlassTypes
            .AnyAsync(
                t => t.WorkshopId == request.WorkshopId && t.Name == name,
                cancellationToken);

        if (duplicate)
            return new CreateGlassTypeDuplicateName();

        var glassType = new GlassType
        {
            WorkshopId = request.WorkshopId,
            Name = name,
            IsActive = true
        };

        context.GlassTypes.Add(glassType);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateGlassTypeSucceeded(ToDto(glassType, []));
    }

    internal static GlassTypeDto ToDto(
        GlassType glassType,
        IReadOnlyList<GlassSpecDto> specs) =>
        new(
            glassType.Id,
            glassType.WorkshopId,
            glassType.Name,
            glassType.IsActive,
            specs);

    internal static GlassSpecDto ToSpecDto(GlassSpec spec) =>
        new(
            spec.Id,
            spec.GlassTypeId,
            spec.ThicknessMm,
            spec.Description,
            spec.PricePerSqm,
            spec.IsActive);
}
