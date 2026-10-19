using CuttingOptimizer.Api.Contracts.Glass;
using CuttingOptimizer.Application.Features.Glass;
using CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassSpec;
using CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassType;
using CuttingOptimizer.Application.Features.Glass.Commands.UpdateGlassSpec;
using CuttingOptimizer.Application.Features.Glass.Commands.UpdateGlassType;
using CuttingOptimizer.Application.Features.Glass.Queries.GetGlassSpecById;
using CuttingOptimizer.Application.Features.Glass.Queries.ListWorkshopGlassTypes;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public sealed class GlassController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpGet("workshops/{workshopId:guid}/glass-types")]
    public async Task<ActionResult<IReadOnlyList<GlassTypeDto>>> List(
        Guid workshopId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var types = await sender.Send(
            new ListWorkshopGlassTypesQuery(workshopId, activeOnly),
            cancellationToken);

        if (types is null)
        {
            return NotFound(new
            {
                message = localizer["WorkshopNotFound"].Value
            });
        }

        return Ok(types);
    }

    [HttpPost("workshops/{workshopId:guid}/glass-types")]
    public async Task<IActionResult> CreateType(
        Guid workshopId,
        [FromBody] CreateGlassTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateGlassTypeCommand(workshopId, request.Name),
            cancellationToken);

        return result switch
        {
            CreateGlassTypeSucceeded ok => Created(
                $"/api/glass-types/{ok.GlassType.Id}",
                ok.GlassType),
            CreateGlassTypeWorkshopNotFound => NotFound(new
            {
                message = localizer["WorkshopNotFound"].Value
            }),
            CreateGlassTypeDuplicateName => Conflict(new
            {
                message = localizer["GlassTypeDuplicateName"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("glass-types/{id:guid}")]
    public async Task<IActionResult> UpdateType(
        Guid id,
        [FromBody] UpdateGlassTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateGlassTypeCommand(id, request.Name, request.IsActive),
            cancellationToken);

        return result switch
        {
            UpdateGlassTypeSucceeded ok => Ok(ok.GlassType),
            UpdateGlassTypeNotFound => NotFound(new
            {
                message = localizer["GlassTypeNotFound"].Value
            }),
            UpdateGlassTypeDuplicateName => Conflict(new
            {
                message = localizer["GlassTypeDuplicateName"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPost("glass-types/{typeId:guid}/specs")]
    public async Task<IActionResult> CreateSpec(
        Guid typeId,
        [FromBody] CreateGlassSpecRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateGlassSpecCommand(
                typeId,
                request.ThicknessMm,
                request.PricePerSqm,
                request.Description),
            cancellationToken);

        return result switch
        {
            CreateGlassSpecSucceeded ok => Created(
                $"/api/glass-specs/{ok.Spec.Id}",
                ok.Spec),
            CreateGlassSpecTypeNotFound => NotFound(new
            {
                message = localizer["GlassTypeNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("glass-specs/{id:guid}")]
    public async Task<IActionResult> UpdateSpec(
        Guid id,
        [FromBody] UpdateGlassSpecRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateGlassSpecCommand(
                id,
                request.ThicknessMm,
                request.PricePerSqm,
                request.Description,
                request.IsActive),
            cancellationToken);

        return result switch
        {
            UpdateGlassSpecSucceeded ok => Ok(ok.Spec),
            UpdateGlassSpecNotFound => NotFound(new
            {
                message = localizer["GlassSpecNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpGet("glass-specs/{id:guid}")]
    public async Task<ActionResult<GlassSpecDto>> GetSpec(
        Guid id,
        CancellationToken cancellationToken)
    {
        var spec = await sender.Send(
            new GetGlassSpecByIdQuery(id),
            cancellationToken);

        if (spec is null)
        {
            return NotFound(new
            {
                message = localizer["GlassSpecNotFound"].Value
            });
        }

        return Ok(spec);
    }
}
