namespace CuttingOptimizer.Application.Features.Glass;

public sealed record GlassSpecDto(
    Guid Id,
    Guid GlassTypeId,
    decimal ThicknessMm,
    string? Description,
    decimal PricePerSqm,
    bool IsActive);

public sealed record GlassTypeDto(
    Guid Id,
    Guid WorkshopId,
    string Name,
    bool IsActive,
    IReadOnlyList<GlassSpecDto> Specs);
