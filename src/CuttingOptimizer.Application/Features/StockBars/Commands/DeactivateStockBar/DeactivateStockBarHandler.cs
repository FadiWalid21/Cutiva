using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.DeactivateStockBar;

public sealed class DeactivateStockBarHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeactivateStockBarCommand, bool>
{
    public async Task<bool> Handle(
        DeactivateStockBarCommand request,
        CancellationToken cancellationToken)
    {
        var stockBar = await context.StockBars
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (stockBar is null)
            return false;

        stockBar.IsActive = false;

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}