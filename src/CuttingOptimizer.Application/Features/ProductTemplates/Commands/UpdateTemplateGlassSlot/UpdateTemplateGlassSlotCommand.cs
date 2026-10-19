using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplateGlassSlot;

/// <summary>
/// Edits glass slot formulas. Authorization: [Authorize] only for MVP.
/// </summary>
public sealed record UpdateTemplateGlassSlotCommand(
    Guid Id,
    string Name,
    int PaneCount,
    decimal WidthCoefW,
    decimal HeightCoefW,
    int OffsetWMm,
    decimal WidthCoefH,
    decimal HeightCoefH,
    int OffsetHMm,
    bool Required,
    bool UseSashWidthDivision)
    : IRequest<UpdateTemplateGlassSlotResult>;

public abstract record UpdateTemplateGlassSlotResult;

public sealed record UpdateTemplateGlassSlotSucceeded(TemplateGlassSlotDto Slot)
    : UpdateTemplateGlassSlotResult;

public sealed record UpdateTemplateGlassSlotNotFound : UpdateTemplateGlassSlotResult;
