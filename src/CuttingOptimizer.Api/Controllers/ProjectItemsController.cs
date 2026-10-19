using CuttingOptimizer.Api.Contracts.ProjectItems;
using CuttingOptimizer.Application.Features.ProjectItems;
using CuttingOptimizer.Application.Features.ProjectItems.Commands.CreateProjectItem;
using CuttingOptimizer.Application.Features.ProjectItems.Commands.DeleteProjectItem;
using CuttingOptimizer.Application.Features.ProjectItems.Commands.DuplicateProjectItem;
using CuttingOptimizer.Application.Features.ProjectItems.Commands.UpdateProjectItem;
using CuttingOptimizer.Application.Features.ProjectItems.Commands.UpsertProjectItemInspection;
using CuttingOptimizer.Application.Features.ProjectItems.Queries.GetProjectItemById;
using CuttingOptimizer.Application.Features.ProjectItems.Queries.ListProjectItemsByProject;
using CuttingOptimizer.Application.Features.Projects.Commands.CalculateProject;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public sealed class ProjectItemsController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpGet("projects/{projectId:guid}/items")]
    public async Task<ActionResult<IReadOnlyList<ProjectItemSummaryDto>>> List(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var items = await sender.Send(
            new ListProjectItemsByProjectQuery(projectId),
            cancellationToken);

        if (items is null)
        {
            return NotFound(new
            {
                message = localizer["ProjectNotFound"].Value
            });
        }

        return Ok(items);
    }

    [HttpPost("projects/{projectId:guid}/items")]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreateProjectItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateProjectItemCommand(
                projectId,
                request.ProductVariantId,
                request.Label,
                request.WidthMm,
                request.HeightMm,
                request.Quantity,
                MapParts(request.PartChoices),
                MapGlass(request.GlassChoices),
                MapAccessories(request.AccessoryChoices),
                MapInspection(request.Inspection)),
            cancellationToken);

        return result switch
        {
            CreateProjectItemSucceeded ok => Created(
                $"/api/project-items/{ok.Item.Id}",
                ok.Item),
            CreateProjectItemProjectNotFound => NotFound(new
            {
                message = localizer["ProjectNotFound"].Value
            }),
            CreateProjectItemVariantNotFound => NotFound(new
            {
                message = localizer["ProductVariantNotFound"].Value
            }),
            CreateProjectItemValidationFailed fail => Conflict(new
            {
                code = fail.Code,
                message = LocalizeValidation(fail.Code)
            }),
            _ => StatusCode(500)
        };
    }

    [HttpGet("project-items/{id:guid}")]
    public async Task<ActionResult<ProjectItemDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var item = await sender.Send(
            new GetProjectItemByIdQuery(id),
            cancellationToken);

        if (item is null)
        {
            return NotFound(new
            {
                message = localizer["ProjectItemNotFound"].Value
            });
        }

        return Ok(item);
    }

    [HttpPut("project-items/{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateProjectItemCommand(
                id,
                request.Label,
                request.WidthMm,
                request.HeightMm,
                request.Quantity,
                MapParts(request.PartChoices),
                MapGlass(request.GlassChoices),
                MapAccessories(request.AccessoryChoices)),
            cancellationToken);

        return result switch
        {
            UpdateProjectItemSucceeded ok => Ok(ok.Item),
            UpdateProjectItemNotFound => NotFound(new
            {
                message = localizer["ProjectItemNotFound"].Value
            }),
            UpdateProjectItemValidationFailed fail => Conflict(new
            {
                code = fail.Code,
                message = LocalizeValidation(fail.Code)
            }),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("project-items/{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteProjectItemCommand(id),
            cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = localizer["ProjectItemNotFound"].Value
            });
        }

        return NoContent();
    }

    [HttpPost("project-items/{id:guid}/duplicate")]
    public async Task<IActionResult> Duplicate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DuplicateProjectItemCommand(id),
            cancellationToken);

        return result switch
        {
            DuplicateProjectItemSucceeded ok => Created(
                $"/api/project-items/{ok.Item.Id}",
                ok.Item),
            DuplicateProjectItemNotFound => NotFound(new
            {
                message = localizer["ProjectItemNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("project-items/{id:guid}/inspection")]
    public async Task<IActionResult> UpsertInspection(
        Guid id,
        [FromBody] UpsertProjectItemInspectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpsertProjectItemInspectionCommand(
                id,
                request.LocationLabel,
                request.MeasuredWidthMm,
                request.MeasuredHeightMm,
                request.InspectedAtUtc,
                request.InspectedBy,
                request.Notes,
                request.Status),
            cancellationToken);

        return result switch
        {
            UpsertProjectItemInspectionSucceeded ok => Ok(ok.Inspection),
            UpsertProjectItemInspectionNotFound => NotFound(new
            {
                message = localizer["ProjectItemNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPost("projects/{id:guid}/calculate")]
    public async Task<IActionResult> Calculate(
        Guid id,
        [FromBody] CalculateProjectRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new CalculateProjectRequest();

        var result = await sender.Send(
            new CalculateProjectCommand(id, request.KerfMm, request.UseRemnants),
            cancellationToken);

        return result switch
        {
            CalculateProjectSucceeded ok => Ok(ok.Result),
            CalculateProjectNotFound => NotFound(new
            {
                message = localizer["ProjectNotFound"].Value
            }),
            CalculateProjectNoItems => Conflict(new
            {
                message = localizer["ProjectHasNoItems"].Value
            }),
            CalculateProjectFailed fail => Conflict(new
            {
                code = fail.Code,
                message = fail.Message
            }),
            _ => StatusCode(500)
        };
    }

    private string LocalizeValidation(string code) =>
        localizer[code].ResourceNotFound
            ? code
            : localizer[code].Value;

    private static IReadOnlyList<ProjectItemPartChoiceInput> MapParts(
        IReadOnlyList<ProjectItemPartChoiceRequest>? choices) =>
        (choices ?? [])
            .Select(c => new ProjectItemPartChoiceInput(c.TemplatePartId, c.MaterialId))
            .ToList();

    private static IReadOnlyList<ProjectItemGlassChoiceInput> MapGlass(
        IReadOnlyList<ProjectItemGlassChoiceRequest>? choices) =>
        (choices ?? [])
            .Select(c => new ProjectItemGlassChoiceInput(
                c.TemplateGlassSlotId,
                c.GlassSpecId))
            .ToList();

    private static IReadOnlyList<ProjectItemAccessoryChoiceInput> MapAccessories(
        IReadOnlyList<ProjectItemAccessoryChoiceRequest>? choices) =>
        (choices ?? [])
            .Select(c => new ProjectItemAccessoryChoiceInput(
                c.TemplateAccessorySlotId,
                c.AccessoryId,
                c.QuantityOverride))
            .ToList();

    private static ProjectItemInspectionInput? MapInspection(
        ProjectItemInspectionRequest? inspection) =>
        inspection is null
            ? null
            : new ProjectItemInspectionInput(
                inspection.LocationLabel,
                inspection.MeasuredWidthMm,
                inspection.MeasuredHeightMm,
                inspection.InspectedAtUtc,
                inspection.InspectedBy,
                inspection.Notes,
                inspection.Status);
}
