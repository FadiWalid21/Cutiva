using CuttingOptimizer.Api.Contracts.ProjectSections;
using CuttingOptimizer.Application.Features.Accessories;
using CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;
using CuttingOptimizer.Application.Features.ProjectSections.Commands.DeleteProjectSection;
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
        var accessories = (request.Accessories ?? [])
            .Select(a => new SectionAccessoryInput(a.AccessoryId, a.Quantity))
            .ToList();

        var result = await sender.Send(
            new CreateProjectSectionCommand(
                projectId,
                request.SectorId,
                request.Name,
                request.HeightMm,
                request.WidthMm,
                request.UnitQuantity,
                accessories),
            cancellationToken);

        return result switch
        {
            CreateProjectSectionSucceeded ok => Created(
                $"/api/projects/{projectId}/sections/{ok.SectionId}",
                new
                {
                    id = ok.SectionId,
                    projectId,
                    request.SectorId,
                    request.Name,
                    request.HeightMm,
                    request.WidthMm,
                    request.UnitQuantity,
                    accessories
                }),
            CreateProjectSectionNotFound => NotFound(new
            {
                message = localizer["ProjectOrSectorNotFound"].Value
            }),
            CreateProjectSectionInvalidAccessories => BadRequest(new
            {
                message = localizer["AccessoryInvalidForWorkshop"].Value
            }),
            _ => StatusCode(500)
        };
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

    [HttpDelete("{sectionId:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteProjectSectionCommand(projectId, sectionId),
            cancellationToken);

        return result switch
        {
            DeleteProjectSectionResult.Deleted => NoContent(),
            DeleteProjectSectionResult.NotFound => NotFound(new
            {
                message = localizer["SectionNotFound"].Value
            }),
            DeleteProjectSectionResult.InUse => Conflict(new
            {
                message = localizer["SectionInUse"].Value
            }),
            _ => StatusCode(500)
        };
    }
}
