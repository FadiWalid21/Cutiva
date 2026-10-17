using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.StockBars.Queries.GetStockBars;

public sealed class GetStockBarsHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetStockBarsQuery, IReadOnlyList<StockBarDto>>
{
    public async Task<IReadOnlyList<StockBarDto>> Handle(
        GetStockBarsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.StockBars
            .AsNoTracking()
            .AsQueryable();

        if (request.MaterialId.HasValue)
        {
            query = query.Where(
                stockBar => stockBar.MaterialId == request.MaterialId.Value);
        }

        if (request.SectorId.HasValue)
        {
            query = query.Where(
                stockBar => stockBar.Material.SectorId == request.SectorId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(
                stockBar => stockBar.IsActive == request.IsActive.Value);
        }

        return await query
            .OrderBy(stockBar => stockBar.Material.Name)
            .ThenBy(stockBar => stockBar.LengthMm)
            .Select(stockBar => new StockBarDto(
                stockBar.Id,
                stockBar.MaterialId,
                stockBar.Material.Code,
                stockBar.Material.Name,
                stockBar.Material.SectorId,
                stockBar.Material.Sector.Code,
                stockBar.Material.Sector.Name,
                stockBar.LengthMm,
                stockBar.UnitPrice,
                stockBar.IsActive))
            .ToListAsync(cancellationToken);
    }
}