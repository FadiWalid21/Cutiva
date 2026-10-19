using CuttingOptimizer.Application.Features.ProductTemplates;
using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplateAccessorySlot;

/// <summary>
/// Edits accessory slot rules. Authorization: [Authorize] only for MVP.
/// </summary>
public sealed record UpdateTemplateAccessorySlotCommand(
    Guid Id,
    string Name,
    AccessoryQuantityRule QuantityRule,
    int Qty,
    Guid? DefaultAccessoryId)
    : IRequest<UpdateTemplateAccessorySlotResult>;

public abstract record UpdateTemplateAccessorySlotResult;

public sealed record UpdateTemplateAccessorySlotSucceeded(TemplateAccessorySlotDto Slot)
    : UpdateTemplateAccessorySlotResult;

public sealed record UpdateTemplateAccessorySlotNotFound : UpdateTemplateAccessorySlotResult;

public sealed record UpdateTemplateAccessorySlotInvalidAccessory
    : UpdateTemplateAccessorySlotResult;
