using CuttingOptimizer.Api.Contracts.Sectors;
using CuttingOptimizer.Application.Features.Sectors.Commands.CreateSector;
using CuttingOptimizer.Application.Features.Sectors.Commands.DeleteSector;
using CuttingOptimizer.Application.Features.Sectors.Commands.UpdateSector;
using CuttingOptimizer.Application.Features.Sectors.Queries.GetSectors;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

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
        var sectorId = await sender.Send(
            new CreateSectorCommand(request.Code, request.Name),
            cancellationToken);

        if (sectorId is null)
        {
            return Conflict(new
            {
                message = localizer["SectorCodeExists"].Value
            });
        }

        return Created(
            $"/api/sectors/{sectorId}",
            new
            {
                id = sectorId,
                code = request.Code.Trim().ToUpperInvariant(),
                request.Name
            });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateSectorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateSectorCommand(id, request.Code, request.Name),
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
