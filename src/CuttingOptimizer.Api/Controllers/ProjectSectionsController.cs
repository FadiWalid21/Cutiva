using CuttingOptimizer.Api.Contracts.ProjectSections;
using CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;
using CuttingOptimizer.Application.Features.ProjectSections.Queries.GetProjectSections;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:guid}/sections")]
public sealed class ProjectSectionsController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
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
                message = localizer["ProjectOrSectorNotFound"].Value
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
                message = localizer["ProjectNotFound"].Value
            });
        }

        return Ok(sections);
    }
}
