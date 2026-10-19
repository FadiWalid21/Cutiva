using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplatePart;

/// <summary>
/// Edits aluminum part formulas. Authorization: [Authorize] only for MVP.
/// </summary>
public sealed record UpdateTemplatePartCommand(
    Guid Id,
    string DisplayName,
    int QtyPerUnit,
    decimal WidthCoef,
    decimal HeightCoef,
    int OffsetMm,
    bool UseSashWidthDivision,
    Guid? DefaultAluminumMaterialId)
    : IRequest<UpdateTemplatePartResult>;

public abstract record UpdateTemplatePartResult;

public sealed record UpdateTemplatePartSucceeded(TemplatePartDto Part)
    : UpdateTemplatePartResult;

public sealed record UpdateTemplatePartNotFound : UpdateTemplatePartResult;

public sealed record UpdateTemplatePartInvalidMaterial : UpdateTemplatePartResult;
