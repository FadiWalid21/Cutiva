using MediatR;

namespace CuttingOptimizer.Application.Features.StockBars.Queries.GetStockBars;

public sealed record GetStockBarsQuery(
    Guid? MaterialId = null,
    Guid? SectorId = null,
    bool? IsActive = null,
    bool? IsRemnant = null
) : IRequest<IReadOnlyList<StockBarDto>>;