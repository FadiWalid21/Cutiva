using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.CreateStockBar;


public sealed class CreateStockBarHandler(
    IApplicationDbContext context)
    : IRequestHandler<CreateStockBarCommand, Guid?>
{
    public async Task<Guid?> Handle(
        CreateStockBarCommand request,
        CancellationToken cancellationToken)
    {
        var materialExists = await context.Materials
            .AnyAsync(
                x => x.Id == request.MaterialId,
                cancellationToken);

        if (!materialExists)
            return null;

        var stockBar = new StockBar
        {
            MaterialId = request.MaterialId,
            LengthMm = request.LengthMm,
            UnitPrice = request.UnitPrice,
            AvailableQuantity = request.AvailableQuantity,
            IsRemnant = false,
            IsActive = true
        };

        context.StockBars.Add(stockBar);

        await context.SaveChangesAsync(cancellationToken);

        return stockBar.Id;
    }
}