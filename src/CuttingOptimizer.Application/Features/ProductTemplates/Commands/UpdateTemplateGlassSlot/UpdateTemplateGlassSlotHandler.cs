using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplateGlassSlot;

public sealed class UpdateTemplateGlassSlotHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateTemplateGlassSlotCommand, UpdateTemplateGlassSlotResult>
{
    public async Task<UpdateTemplateGlassSlotResult> Handle(
        UpdateTemplateGlassSlotCommand request,
        CancellationToken cancellationToken)
    {
        var slot = await context.TemplateGlassSlots
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (slot is null)
            return new UpdateTemplateGlassSlotNotFound();

        slot.Name = request.Name.Trim();
        slot.PaneCount = request.PaneCount;
        slot.WidthCoefW = request.WidthCoefW;
        slot.HeightCoefW = request.HeightCoefW;
        slot.OffsetWMm = request.OffsetWMm;
        slot.WidthCoefH = request.WidthCoefH;
        slot.HeightCoefH = request.HeightCoefH;
        slot.OffsetHMm = request.OffsetHMm;
        slot.Required = request.Required;
        slot.UseSashWidthDivision = request.UseSashWidthDivision;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateTemplateGlassSlotSucceeded(
            ProductTemplateMapper.ToGlassSlotDto(slot));
    }
}
