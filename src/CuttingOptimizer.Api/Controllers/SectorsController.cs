using CuttingOptimizer.Api.Contracts.Sectors;
using CuttingOptimizer.Application.Features.Sectors.Commands.CreateSector;
using CuttingOptimizer.Application.Features.Sectors.Commands.DeleteSector;
using CuttingOptimizer.Application.Features.Sectors.Commands.UpdateSector;
using CuttingOptimizer.Application.Features.Sectors.Queries.GetSectors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/sectors")]
public sealed class SectorsController(ISender sender) : ControllerBase
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
                message = "A sector with this code already exists."
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
                message = "Sector not found."
            }),
            UpdateSectorResult.CodeAlreadyExists => Conflict(new
            {
                message = "A sector with this code already exists."
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
                message = "Sector not found."
            }),
            DeleteSectorResult.InUse => Conflict(new
            {
                message = "Cannot delete a sector used by project sections."
            }),
            _ => StatusCode(500)
        };
    }
}