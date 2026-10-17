namespace CuttingOptimizer.Application.Features.StockBars.Queries;

public sealed record StockBarDto(
    Guid Id,
    Guid MaterialId,
    string MaterialCode,
    string MaterialName,
    Guid SectorId,
    string SectorCode,
    string SectorName,
    int LengthMm,
    decimal UnitPrice,
    bool IsActive
);