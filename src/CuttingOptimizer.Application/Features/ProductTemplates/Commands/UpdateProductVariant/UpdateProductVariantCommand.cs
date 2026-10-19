using CuttingOptimizer.Application.Features.ProductTemplates;
using MediatR;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateProductVariant;

/// <summary>
/// Updates variant metadata editable from the templates UI.
/// Authorization: [Authorize] only for MVP (same as materials); Admin/WorkshopOwner
/// role checks can be tightened later when workshop claims are standardized.
/// </summary>
public sealed record UpdateProductVariantCommand(
    Guid Id,
    string Name,
    int? SashCount,
    int? OverlapMm,
    bool IsActive)
    : IRequest<UpdateProductVariantResult>;

public abstract record UpdateProductVariantResult;

public sealed record UpdateProductVariantSucceeded(ProductVariantDto Variant)
    : UpdateProductVariantResult;

public sealed record UpdateProductVariantNotFound : UpdateProductVariantResult;
