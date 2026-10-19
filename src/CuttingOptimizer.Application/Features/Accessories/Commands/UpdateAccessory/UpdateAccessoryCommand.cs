using CuttingOptimizer.Application.Features.Accessories;
using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.Accessories.Commands.UpdateAccessory;

public sealed record UpdateAccessoryCommand(
    Guid Id,
    string Name,
    decimal UnitPrice,
    string? Brand = null,
    AccessoryUnit Unit = AccessoryUnit.Piece,
    bool IsActive = true)
    : IRequest<UpdateAccessoryResult>;

public abstract record UpdateAccessoryResult;

public sealed record UpdateAccessorySucceeded(AccessoryDto Accessory)
    : UpdateAccessoryResult;

public sealed record UpdateAccessoryNotFound : UpdateAccessoryResult;

public sealed record UpdateAccessoryDuplicateName : UpdateAccessoryResult;
