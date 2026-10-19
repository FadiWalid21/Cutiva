using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Features.Accessories;

public sealed record AccessoryDto(
    Guid Id,
    Guid WorkshopId,
    string Name,
    string? Brand,
    AccessoryUnit Unit,
    decimal UnitPrice,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record SectionAccessoryInput(
    Guid AccessoryId,
    int Quantity);

public sealed record SectionAccessoryDto(
    Guid AccessoryId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record QuoteAccessoryLineDto(
    Guid AccessoryId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
