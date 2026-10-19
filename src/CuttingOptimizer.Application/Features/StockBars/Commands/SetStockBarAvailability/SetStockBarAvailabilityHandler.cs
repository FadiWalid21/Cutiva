using CuttingOptimizer.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.SetStockBarAvailability;

public sealed class SetStockBarAvailabilityHandler(
    IApplicationDbContext context)
    : IRequestHandler<SetStockBarAvailabilityCommand, bool>
{
    public async Task<bool> Handle(
        SetStockBarAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        var stockBar = await context.StockBars
            .FirstOrDefaultAsync(
                x => x.Id == request.Id && !x.IsRemnant,
                cancellationToken);

        if (stockBar is null)
            return false;

        stockBar.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
