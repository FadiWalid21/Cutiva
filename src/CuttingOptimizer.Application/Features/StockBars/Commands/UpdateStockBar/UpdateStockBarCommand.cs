using MediatR;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.UpdateStockBar;

public sealed record UpdateStockBarCommand(
    Guid Id,
    Guid MaterialId,
    int LengthMm,
    decimal UnitPrice,
    int AvailableQuantity
) : IRequest<bool>;