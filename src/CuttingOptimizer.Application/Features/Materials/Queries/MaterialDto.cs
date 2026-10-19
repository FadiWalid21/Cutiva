using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Features.Materials.Queries;

public sealed record MaterialDto(
    Guid Id,
    Guid SectorId,
    string SectorCode,
    string SectorName,
    string Code,
    string Name,
    string? Description,
    MaterialCategory Category,
    decimal? ThicknessMm,
    bool IsActive
);
