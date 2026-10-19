using MediatR;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.SetStockBarAvailability;

public sealed record SetStockBarAvailabilityCommand(
    Guid Id,
    bool IsActive) : IRequest<bool>;
