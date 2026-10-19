using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Glass;
using CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassType;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Glass.Commands.UpdateGlassType;

public sealed class UpdateGlassTypeHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateGlassTypeCommand, UpdateGlassTypeResult>
{
    public async Task<UpdateGlassTypeResult> Handle(
        UpdateGlassTypeCommand request,
        CancellationToken cancellationToken)
    {
        var glassType = await context.GlassTypes
            .Include(t => t.Specs)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (glassType is null)
            return new UpdateGlassTypeNotFound();

        var name = request.Name.Trim();
        var duplicate = await context.GlassTypes
            .AnyAsync(
                t => t.WorkshopId == glassType.WorkshopId
                     && t.Name == name
                     && t.Id != glassType.Id,
                cancellationToken);

        if (duplicate)
            return new UpdateGlassTypeDuplicateName();

        glassType.Name = name;
        glassType.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        var specs = glassType.Specs
            .OrderBy(s => s.ThicknessMm)
            .Select(CreateGlassTypeHandler.ToSpecDto)
            .ToList();

        return new UpdateGlassTypeSucceeded(
            CreateGlassTypeHandler.ToDto(glassType, specs));
    }
}
