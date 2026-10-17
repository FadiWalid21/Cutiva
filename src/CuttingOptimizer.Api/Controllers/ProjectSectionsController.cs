using CuttingOptimizer.Api.Contracts.ProjectSections;
using CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;
using CuttingOptimizer.Application.Features.ProjectSections.Queries.GetProjectSections;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/sections")]
public sealed class ProjectSectionsController(ISender sender)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreateProjectSectionRequest request,
        CancellationToken cancellationToken)
    {
        var sectionId = await sender.Send(
            new CreateProjectSectionCommand(
                projectId,
                request.SectorId,
                request.Name),
            cancellationToken);

        if (sectionId is null)
        {
            return NotFound(new
            {
                message = "Project or sector not found."
            });
        }

        return Created(
            $"/api/projects/{projectId}/sections/{sectionId}",
            new
            {
                id = sectionId,
                projectId,
                request.SectorId,
                request.Name
            });
    }
    
    
    [HttpGet]
    public async Task<IActionResult> GetAll(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var sections = await sender.Send(
            new GetProjectSectionsQuery(projectId),
            cancellationToken);

        if (sections is null)
        {
            return NotFound(new
            {
                message = "Project not found."
            });
        }

        return Ok(sections);
    }
}