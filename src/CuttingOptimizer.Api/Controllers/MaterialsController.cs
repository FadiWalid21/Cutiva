using CuttingOptimizer.Api.Contracts.Materials;
using CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;
using CuttingOptimizer.Application.Features.Materials.Commands.DeleteMaterial;
using CuttingOptimizer.Application.Features.Materials.Commands.UpdateMaterial;
using CuttingOptimizer.Application.Features.Materials.Queries;
using CuttingOptimizer.Application.Features.Materials.Queries.GetMaterialById;
using CuttingOptimizer.Application.Features.Materials.Queries.GetMaterials;
using CuttingOptimizer.Application.Resources;
using CuttingOptimizer.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/materials")]
public sealed class MaterialsController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateMaterialCommand(
                request.SectorId,
                request.Code,
                request.Name,
                request.Description,
                request.Category,
                request.ThicknessMm),
            cancellationToken);

        return result switch
        {
            CreateMaterialSucceeded ok => Created(
                $"/api/materials/{ok.Id}",
                new
                {
                    id = ok.Id,
                    request.SectorId,
                    code = ok.Code,
                    request.Name,
                    request.Description,
                    request.Category,
                    request.ThicknessMm,
                    reactivated = ok.Reactivated
                }),
            CreateMaterialSectorNotFound => NotFound(new
            {
                message = localizer["SectorNotFound"].Value
            }),
            CreateMaterialCodeExists exists => Conflict(new
            {
                message = localizer["MaterialCodeExists", exists.Code].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateMaterialCommand(
                id,
                request.Name,
                request.Description,
                request.Category,
                request.ThicknessMm,
                request.IsActive),
            cancellationToken);

        return result switch
        {
            UpdateMaterialSucceeded ok => Ok(ok.Material),
            UpdateMaterialNotFound => NotFound(new
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
        var result = await sender.Send(new DeleteMaterialCommand(id), cancellationToken);
        return result switch
        {
            DeleteMaterialSucceeded => NoContent(),
            DeleteMaterialNotFound => NotFound(new
            {
                message = localizer["MaterialNotFound"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MaterialDto>>> GetAll(
        [FromQuery] Guid? sectorId,
        [FromQuery] MaterialCategory? category,
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken)
    {
        var materials = await sender.Send(
            new GetMaterialsQuery(sectorId, category, activeOnly),
            cancellationToken);

        return Ok(materials);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MaterialDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var material = await sender.Send(
            new GetMaterialByIdQuery(id),
            cancellationToken);

        if (material is null)
        {
            return NotFound(new
            {
                message = localizer["MaterialNotFound"].Value
            });
        }

        return Ok(material);
    }
}
