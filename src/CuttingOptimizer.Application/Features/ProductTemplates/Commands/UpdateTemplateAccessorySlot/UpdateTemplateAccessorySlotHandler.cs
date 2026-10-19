using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplateAccessorySlot;

public sealed class UpdateTemplateAccessorySlotHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateTemplateAccessorySlotCommand, UpdateTemplateAccessorySlotResult>
{
    public async Task<UpdateTemplateAccessorySlotResult> Handle(
        UpdateTemplateAccessorySlotCommand request,
        CancellationToken cancellationToken)
    {
        var slot = await context.TemplateAccessorySlots
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slot is null)
            return new UpdateTemplateAccessorySlotNotFound();

        if (request.DefaultAccessoryId is { } accessoryId)
        {
            var accessoryOk = await context.Accessories.AnyAsync(
                a => a.Id == accessoryId && a.IsActive,
                cancellationToken);

            if (!accessoryOk)
                return new UpdateTemplateAccessorySlotInvalidAccessory();
        }

        slot.Name = request.Name.Trim();
        slot.QuantityRule = request.QuantityRule;
        slot.Qty = request.Qty;
        slot.DefaultAccessoryId = request.DefaultAccessoryId;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateTemplateAccessorySlotSucceeded(
            ProductTemplateMapper.ToAccessorySlotDto(slot));
    }
}
