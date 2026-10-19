using CuttingOptimizer.Api.Contracts.ProductTemplates;
using CuttingOptimizer.Application.Features.ProductTemplates;
using CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateProductVariant;
using CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplateAccessorySlot;
using CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplateGlassSlot;
using CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplatePart;
using CuttingOptimizer.Application.Features.ProductTemplates.Queries.GetProductVariantById;
using CuttingOptimizer.Application.Features.ProductTemplates.Queries.ListProductTypes;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

/// <summary>
/// Product type/variant template catalog.
/// Updates use [Authorize] only for MVP (same pattern as materials).
/// Admin/WorkshopOwner role gating can be added when workshop claims are standardized.
/// </summary>
[Authorize]
[ApiController]
[Route("api/product-templates")]
public sealed class ProductTemplatesController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<ProductTypeDto>>> ListTypes(
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var types = await sender.Send(
            new ListProductTypesQuery(activeOnly),
            cancellationToken);

        return Ok(types);
    }

    [HttpGet("variants/{id:guid}")]
    public async Task<ActionResult<ProductVariantDto>> GetVariant(
        Guid id,
        CancellationToken cancellationToken)
    {
        var variant = await sender.Send(
            new GetProductVariantByIdQuery(id),
            cancellationToken);

        if (variant is null)
        {
            return NotFound(new
            {
                message = localizer["ProductVariantNotFound"].Value
            });
        }

        return Ok(variant);
    }

    [HttpPut("variants/{id:guid}")]
    public async Task<IActionResult> UpdateVariant(
        Guid id,
        [FromBody] UpdateProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateProductVariantCommand(
                id,
                request.Name,
                request.SashCount,
                request.OverlapMm,
                request.IsActive),
            cancellationToken);

        return result switch
        {
            UpdateProductVariantSucceeded ok => Ok(ok.Variant),
            UpdateProductVariantNotFound => NotFound(new
            {
                message = localizer["ProductVariantNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("parts/{id:guid}")]
    public async Task<IActionResult> UpdatePart(
        Guid id,
        [FromBody] UpdateTemplatePartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTemplatePartCommand(
                id,
                request.DisplayName,
                request.QtyPerUnit,
                request.WidthCoef,
                request.HeightCoef,
                request.OffsetMm,
                request.UseSashWidthDivision,
                request.DefaultAluminumMaterialId),
            cancellationToken);

        return result switch
        {
            UpdateTemplatePartSucceeded ok => Ok(ok.Part),
            UpdateTemplatePartNotFound => NotFound(new
            {
                message = localizer["TemplatePartNotFound"].Value
            }),
            UpdateTemplatePartInvalidMaterial => Conflict(new
            {
                message = localizer["InvalidAluminumMaterial"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("glass-slots/{id:guid}")]
    public async Task<IActionResult> UpdateGlassSlot(
        Guid id,
        [FromBody] UpdateTemplateGlassSlotRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTemplateGlassSlotCommand(
                id,
                request.Name,
                request.PaneCount,
                request.WidthCoefW,
                request.HeightCoefW,
                request.OffsetWMm,
                request.WidthCoefH,
                request.HeightCoefH,
                request.OffsetHMm,
                request.Required,
                request.UseSashWidthDivision),
            cancellationToken);

        return result switch
        {
            UpdateTemplateGlassSlotSucceeded ok => Ok(ok.Slot),
            UpdateTemplateGlassSlotNotFound => NotFound(new
            {
                message = localizer["TemplateGlassSlotNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("accessory-slots/{id:guid}")]
    public async Task<IActionResult> UpdateAccessorySlot(
        Guid id,
        [FromBody] UpdateTemplateAccessorySlotRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTemplateAccessorySlotCommand(
                id,
                request.Name,
                request.QuantityRule,
                request.Qty,
                request.DefaultAccessoryId),
            cancellationToken);

        return result switch
        {
            UpdateTemplateAccessorySlotSucceeded ok => Ok(ok.Slot),
            UpdateTemplateAccessorySlotNotFound => NotFound(new
            {
                message = localizer["TemplateAccessorySlotNotFound"].Value
            }),
            UpdateTemplateAccessorySlotInvalidAccessory => Conflict(new
            {
                message = localizer["InvalidAccessory"].Value
            }),
            _ => StatusCode(500)
        };
    }
}
