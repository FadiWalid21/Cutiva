using MediatR;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.DeactivateStockBar;

public sealed record DeactivateStockBarCommand(Guid Id) : IRequest<bool>;
