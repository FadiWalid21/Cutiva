using CuttingOptimizer.Api.Contracts.Accessories;
using CuttingOptimizer.Application.Features.Accessories;
using CuttingOptimizer.Application.Features.Accessories.Commands.CreateAccessory;
using CuttingOptimizer.Application.Features.Accessories.Commands.DeleteAccessory;
using CuttingOptimizer.Application.Features.Accessories.Commands.UpdateAccessory;
using CuttingOptimizer.Application.Features.Accessories.Queries.ListWorkshopAccessories;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public sealed class AccessoriesController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpGet("workshops/{workshopId:guid}/accessories")]
    public async Task<ActionResult<IReadOnlyList<AccessoryDto>>> List(
        Guid workshopId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var accessories = await sender.Send(
            new ListWorkshopAccessoriesQuery(workshopId, activeOnly),
            cancellationToken);

        if (accessories is null)
        {
            return NotFound(new
            {
                message = localizer["WorkshopNotFound"].Value
            });
        }

        return Ok(accessories);
    }

    [HttpPost("workshops/{workshopId:guid}/accessories")]
    public async Task<IActionResult> Create(
        Guid workshopId,
        [FromBody] CreateAccessoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateAccessoryCommand(
                workshopId,
                request.Name,
                request.UnitPrice,
                request.Brand,
                request.Unit),
            cancellationToken);

        return result switch
        {
            CreateAccessorySucceeded ok => Created(
                $"/api/accessories/{ok.Accessory.Id}",
                ok.Accessory),
            CreateAccessoryWorkshopNotFound => NotFound(new
            {
                message = localizer["WorkshopNotFound"].Value
            }),
            CreateAccessoryDuplicateName => Conflict(new
            {
                message = localizer["AccessoryDuplicateName"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("accessories/{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAccessoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateAccessoryCommand(
                id,
                request.Name,
                request.UnitPrice,
                request.Brand,
                request.Unit,
                request.IsActive),
            cancellationToken);

        return result switch
        {
            UpdateAccessorySucceeded ok => Ok(ok.Accessory),
            UpdateAccessoryNotFound => NotFound(new
            {
                message = localizer["AccessoryNotFound"].Value
            }),
            UpdateAccessoryDuplicateName => Conflict(new
            {
                message = localizer["AccessoryDuplicateName"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("accessories/{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteAccessoryCommand(id),
            cancellationToken);

        return result switch
        {
            DeleteAccessoryResult.Deleted => NoContent(),
            DeleteAccessoryResult.Deactivated => Ok(new
            {
                deactivated = true,
                message = localizer["AccessoryDeactivated"].Value
            }),
            DeleteAccessoryResult.NotFound => NotFound(new
            {
                message = localizer["AccessoryNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }
}
