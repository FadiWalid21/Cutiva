using CuttingOptimizer.Api.Contracts.Sectors;
using CuttingOptimizer.Application.Features.Sectors.Commands.CreateSector;
using CuttingOptimizer.Application.Features.Sectors.Commands.DeleteSector;
using CuttingOptimizer.Application.Features.Sectors.Commands.UpdateSector;
using CuttingOptimizer.Application.Features.Sectors.Queries.GetSectors;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/sectors")]
public sealed class SectorsController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var sectors = await sender.Send(
            new GetSectorsQuery(),
            cancellationToken);

        return Ok(sectors);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSectorRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = await sender.Send(
            new CreateSectorCommand(
                request.Code,
                request.Name,
                request.MaterialId),
            cancellationToken);

        return outcome.Status switch
        {
            CreateSectorStatus.Created => Created(
                $"/api/sectors/{outcome.Id}",
                new
                {
                    id = outcome.Id,
                    code = request.Code.Trim().ToUpperInvariant(),
                    name = request.Name.Trim(),
                    materialId = request.MaterialId
                }),
            CreateSectorStatus.CodeAlreadyExists => Conflict(new
            {
                message = localizer["SectorCodeExists"].Value
            }),
            CreateSectorStatus.MaterialNotFound => BadRequest(new
            {
                message = localizer["MaterialNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateSectorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateSectorCommand(
                id,
                request.Code,
                request.Name,
                request.MaterialId),
            cancellationToken);

        return result switch
        {
            UpdateSectorResult.Updated => NoContent(),
            UpdateSectorResult.NotFound => NotFound(new
            {
                message = localizer["SectorNotFound"].Value
            }),
            UpdateSectorResult.CodeAlreadyExists => Conflict(new
            {
                message = localizer["SectorCodeExists"].Value
            }),
            UpdateSectorResult.MaterialNotFound => BadRequest(new
            {
                message = localizer["MaterialNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteSectorCommand(id),
            cancellationToken);

        return result switch
        {
            DeleteSectorResult.Deleted => NoContent(),
            DeleteSectorResult.NotFound => NotFound(new
            {
                message = localizer["SectorNotFound"].Value
            }),
            DeleteSectorResult.InUse => Conflict(new
            {
                message = localizer["SectorInUse"].Value
            }),
            _ => StatusCode(500)
        };
    }
}
