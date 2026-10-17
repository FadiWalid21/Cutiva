
using CuttingOptimizer.Api.Contracts.StockBars;
using CuttingOptimizer.Application.Features.StockBars.Commands.CreateStockBar;
using CuttingOptimizer.Application.Features.StockBars.Commands.UpdateStockBar;
using CuttingOptimizer.Application.Features.StockBars.Commands.DeactivateStockBar;
using CuttingOptimizer.Application.Features.StockBars.Queries;
using CuttingOptimizer.Application.Features.StockBars.Queries.GetStockBars;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/stock-bars")]
public sealed class StockBarsController(
    ISender sender) : ControllerBase
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
                message = "The specified material was not found."
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
        CancellationToken cancellationToken)
    {
        var stockBars = await sender.Send(
            new GetStockBarsQuery(materialId, sectorId, isActive),
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
                message = "Stock bar or material was not found."
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
                message = "Stock bar not found."
            });
        }

        return NoContent();
    }
}