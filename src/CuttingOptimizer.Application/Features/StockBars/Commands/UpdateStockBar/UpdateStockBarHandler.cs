using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.UpdateStockBar;

public sealed class UpdateStockBarHandler(
    IApplicationDbContext context)
    : IRequestHandler<UpdateStockBarCommand, bool>
{
    public async Task<bool> Handle(
        UpdateStockBarCommand request,
        CancellationToken cancellationToken)
    {
        var stockBar = await context.StockBars
            .FirstOrDefaultAsync(
                x => x.Id == request.Id && !x.IsRemnant,
                cancellationToken);

        if (stockBar is null)
            return false;

        var materialExists = await context.Materials
            .AnyAsync(
                x => x.Id == request.MaterialId,
                cancellationToken);

        if (!materialExists)
            return false;

        stockBar.MaterialId = request.MaterialId;
        stockBar.LengthMm = request.LengthMm;
        stockBar.UnitPrice = request.UnitPrice;
        stockBar.AvailableQuantity = request.AvailableQuantity;

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}