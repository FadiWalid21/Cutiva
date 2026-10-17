using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Queries.GetRequiredCuts;

public sealed class GetRequiredCutsHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetRequiredCutsQuery, IReadOnlyList<RequiredCutDto>>
{
    public async Task<IReadOnlyList<RequiredCutDto>> Handle(
        GetRequiredCutsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.RequiredCuts
            .AsNoTracking()
            .AsQueryable();

        if (request.ProjectSectionId.HasValue)
        {
            query = query.Where(
                x => x.ProjectSectionId == request.ProjectSectionId.Value);
        }

        if (request.MaterialId.HasValue)
        {
            query = query.Where(
                x => x.MaterialId == request.MaterialId.Value);
        }

        return await query
            .OrderBy(x => x.Material.Name)
            .ThenBy(x => x.LengthMm)
            .Select(x => new RequiredCutDto(
                x.Id,
                x.ProjectSectionId,
                x.ProjectSection.Name,
                x.MaterialId,
                x.Material.Code,
                x.Material.Name,
                x.Name,
                x.LengthMm,
                x.Quantity,
                (long)x.LengthMm * x.Quantity))
            .ToListAsync(cancellationToken);
    }
}