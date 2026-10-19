using CuttingOptimizer.Application.Features.Accessories;
using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.Accessories.Commands.CreateAccessory;

public sealed record CreateAccessoryCommand(
    Guid WorkshopId,
    string Name,
    decimal UnitPrice,
    string? Brand = null,
    AccessoryUnit Unit = AccessoryUnit.Piece)
    : IRequest<CreateAccessoryResult>;

public abstract record CreateAccessoryResult;

public sealed record CreateAccessorySucceeded(AccessoryDto Accessory)
    : CreateAccessoryResult;

public sealed record CreateAccessoryWorkshopNotFound : CreateAccessoryResult;

public sealed record CreateAccessoryDuplicateName : CreateAccessoryResult;
