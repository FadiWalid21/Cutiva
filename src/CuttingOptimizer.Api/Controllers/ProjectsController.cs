using CuttingOptimizer.Api.Contracts.Projects;
using CuttingOptimizer.Application.Features.Projects;
using CuttingOptimizer.Application.Features.Projects.Commands.DeleteProject;
using CuttingOptimizer.Application.Features.Projects.Commands.UpdateProject;
using CuttingOptimizer.Application.Features.Projects.Queries.GetProjects;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workshops/{workshopId:guid}/projects")]
public sealed class ProjectsController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        Guid workshopId,
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var projectId = await sender.Send(
            new CreateProjectCommand(
                workshopId,
                request.Name,
                request.CustomerName),
            cancellationToken);

        if (projectId is null)
        {
            return NotFound(new
            {
                message = localizer["WorkshopNotFound"].Value
            });
        }

        return Created(
            $"/api/projects/{projectId}",
            new
            {
                id = projectId,
                workshopId,
                request.Name,
                request.CustomerName
            });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid workshopId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetProjectsQuery(workshopId, page, pageSize),
            cancellationToken);

        if (result is null)
            return NotFound(new { message = localizer["WorkshopNotFound"].Value });

        return Ok(result);
    }

    [HttpPut("{projectId:guid}")]
    public async Task<IActionResult> Update(
        Guid workshopId,
        Guid projectId,
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdateProjectCommand(
                workshopId,
                projectId,
                request.Name,
                request.CustomerName),
            cancellationToken);

        if (!updated)
            return NotFound(new { message = localizer["ProjectNotFound"].Value });

        return NoContent();
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> Delete(
        Guid workshopId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteProjectCommand(workshopId, projectId),
            cancellationToken);

        if (!deleted)
            return NotFound(new { message = localizer["ProjectNotFound"].Value });

        return NoContent();
    }
}
