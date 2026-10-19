using MediatR;

namespace CuttingOptimizer.Application.Features.StockBars.Commands.DeleteStockBar;

public sealed record DeleteStockBarCommand(Guid Id)
    : IRequest<DeleteStockBarResult>;

public abstract record DeleteStockBarResult;

public sealed record DeleteStockBarSucceeded : DeleteStockBarResult;

public sealed record DeleteStockBarNotFound : DeleteStockBarResult;

public sealed record DeleteStockBarInUse : DeleteStockBarResult;
