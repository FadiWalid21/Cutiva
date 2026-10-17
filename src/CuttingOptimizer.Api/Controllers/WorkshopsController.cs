using CuttingOptimizer.Api.Contracts.Workshops;
using CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;
using CuttingOptimizer.Application.Features.Workshops.Commands.DeleteWorkshop;
using CuttingOptimizer.Application.Features.Workshops.Commands.UpdateWorkshop;
using CuttingOptimizer.Application.Features.Workshops.Queries.GetWorkshopById;
using CuttingOptimizer.Application.Features.Workshops.Queries.GetWorkshops;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WorkshopsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkshopRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWorkshopCommand(request.Name);

        var workshopId = await sender.Send(command, cancellationToken);

        return Created($"/api/workshops/{workshopId}", new
        {
            id = workshopId
        });
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workshop = await sender.Send(
            new GetWorkshopByIdQuery(id),
            cancellationToken);

        if (workshop is null)
            return NotFound();

        return Ok(workshop);
    }
    
    
    
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetWorkshopsQuery(page, pageSize),
            cancellationToken);

        return Ok(result);
    }
    
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateWorkshopRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdateWorkshopCommand(id, request.Name),
            cancellationToken);

        if (!updated)
            return NotFound();

        return NoContent();
    }
    
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteWorkshopCommand(id),
            cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}