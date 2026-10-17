using MediatR;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.CreateStockBar;

public sealed record CreateStockBarCommand(
    Guid MaterialId,
    int LengthMm,
    decimal UnitPrice,
    int AvailableQuantity
) : IRequest<Guid?>;