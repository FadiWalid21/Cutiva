using CuttingOptimizer.Api.Contracts.RequiredCuts;
using CuttingOptimizer.Application.Features.RequiredCuts.Commands.CreateRequiredCut;
using CuttingOptimizer.Application.Features.RequiredCuts.Commands.DeleteRequiredCut;
using CuttingOptimizer.Application.Features.RequiredCuts.Commands.UpdateRequiredCut;
using CuttingOptimizer.Application.Features.RequiredCuts.Queries.GetRequiredCuts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/required-cuts")]
public sealed class RequiredCutsController(
    ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRequiredCutRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            new CreateRequiredCutCommand(
                request.ProjectSectionId,
                request.MaterialId,
                request.Name,
                request.LengthMm,
                request.Quantity),
            cancellationToken);

        if (id is null)
        {
            return NotFound(new
            {
                message = "Project section or matching material was not found."
            });
        }

        return Created(
            "/api/required-cuts",
            new
            {
                id,
                request.ProjectSectionId,
                request.MaterialId,
                request.Name,
                request.LengthMm,
                request.Quantity,
                TotalLengthMm = (long)request.LengthMm * request.Quantity
            });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? projectSectionId,
        [FromQuery] Guid? materialId,
        CancellationToken cancellationToken)
    {
        var cuts = await sender.Send(
            new GetRequiredCutsQuery(projectSectionId, materialId),
            cancellationToken);

        return Ok(cuts);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateRequiredCutRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdateRequiredCutCommand(
                id,
                request.ProjectSectionId,
                request.MaterialId,
                request.Name,
                request.LengthMm,
                request.Quantity),
            cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Required cut, project section, or matching material was not found."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteRequiredCutCommand(id),
            cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Required cut not found."
            });
        }

        return NoContent();
    }
}