using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.DeleteStockBar;

public sealed class DeleteStockBarHandler(
    IApplicationDbContext context)
    : IRequestHandler<DeleteStockBarCommand, DeleteStockBarResult>
{
    public async Task<DeleteStockBarResult> Handle(
        DeleteStockBarCommand request,
        CancellationToken cancellationToken)
    {
        var stockBar = await context.StockBars
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (stockBar is null)
            return new DeleteStockBarNotFound();

        var inUse = await context.CuttingPlanBars
            .AsNoTracking()
            .AnyAsync(
                bar => bar.StockBarId == request.Id,
                cancellationToken);

        if (inUse)
            return new DeleteStockBarInUse();

        context.StockBars.Remove(stockBar);
        await context.SaveChangesAsync(cancellationToken);

        return new DeleteStockBarSucceeded();
    }
}
