using CuttingOptimizer.Api.Contracts.StockBars;
using CuttingOptimizer.Application.Features.StockBars.Commands.CreateStockBar;
using CuttingOptimizer.Application.Features.StockBars.Commands.UpdateStockBar;
using CuttingOptimizer.Application.Features.StockBars.Commands.DeactivateStockBar;
using CuttingOptimizer.Application.Features.StockBars.Commands.DeleteStockBar;
using CuttingOptimizer.Application.Features.StockBars.Commands.SetStockBarAvailability;
using CuttingOptimizer.Application.Features.StockBars.Queries;
using CuttingOptimizer.Application.Features.StockBars.Queries.GetStockBars;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/stock-bars")]
public sealed class StockBarsController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateStockBarRequest request,
        CancellationToken cancellationToken)
    {
        var stockBarId = await sender.Send(
            new CreateStockBarCommand(
                request.MaterialId,
                request.LengthMm,
                request.UnitPrice,
                request.AvailableQuantity),
            cancellationToken);

        if (stockBarId is null)
        {
            return NotFound(new
            {
                message = localizer["MaterialNotFoundSimple"].Value
            });
        }

        return Created(
            $"/api/stock-bars/{stockBarId}",
            new
            {
                id = stockBarId,
                request.MaterialId,
                request.LengthMm,
                request.UnitPrice,
                request.AvailableQuantity
            });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StockBarDto>>> GetAll(
        [FromQuery] Guid? materialId,
        [FromQuery] Guid? sectorId,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isRemnant,
        CancellationToken cancellationToken)
    {
        var stockBars = await sender.Send(
            new GetStockBarsQuery(materialId, sectorId, isActive, isRemnant),
            cancellationToken);

        return Ok(stockBars);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateStockBarRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdateStockBarCommand(
                id,
                request.MaterialId,
                request.LengthMm,
                request.UnitPrice,
                request.AvailableQuantity),
            cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = localizer["StockBarOrMaterialNotFound"].Value
            });
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/availability")]
    public async Task<IActionResult> SetAvailability(
        Guid id,
        [FromBody] SetStockBarAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new SetStockBarAvailabilityCommand(id, request.IsActive),
            cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = localizer["StockBarNotFound"].Value
            });
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deactivated = await sender.Send(
            new DeactivateStockBarCommand(id),
            cancellationToken);

        if (!deactivated)
        {
            return NotFound(new
            {
                message = localizer["StockBarNotFound"].Value
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteStockBarCommand(id),
            cancellationToken);

        return result switch
        {
            DeleteStockBarSucceeded => NoContent(),
            DeleteStockBarInUse => Conflict(new
            {
                message = localizer["StockBarInUse"].Value
            }),
            _ => NotFound(new
            {
                message = localizer["StockBarNotFound"].Value
            })
        };
    }
}
