using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.CreateRequiredCut;

public sealed class CreateRequiredCutHandler(
    IApplicationDbContext context)
    : IRequestHandler<CreateRequiredCutCommand, Guid?>
{
    public async Task<Guid?> Handle(
        CreateRequiredCutCommand request,
        CancellationToken cancellationToken)
    {
        var sectionExists = await context.ProjectSections
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProjectSectionId, cancellationToken);

        if (!sectionExists)
            return null;

        var materialExists = await context.Materials
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.MaterialId, cancellationToken);

        if (!materialExists)
            return null;

        var requiredCut = new RequiredCut
        {
            ProjectSectionId = request.ProjectSectionId,
            MaterialId = request.MaterialId,
            Name = request.Name.Trim(),
            LengthMm = request.LengthMm,
            Quantity = request.Quantity
        };

        context.RequiredCuts.Add(requiredCut);

        await context.SaveChangesAsync(cancellationToken);

        return requiredCut.Id;
    }
}
