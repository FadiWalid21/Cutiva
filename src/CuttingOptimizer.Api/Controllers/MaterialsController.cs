using CuttingOptimizer.Api.Contracts.Materials;
using CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;
using CuttingOptimizer.Application.Features.Materials.Queries;
using CuttingOptimizer.Application.Features.Materials.Queries.GetMaterialById;
using CuttingOptimizer.Application.Features.Materials.Queries.GetMaterials;
using CuttingOptimizer.Application.Resources;
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
        var materialId = await sender.Send(
            new CreateMaterialCommand(
                request.SectorId,
                request.Code,
                request.Name,
                request.Description),
            cancellationToken);

        if (materialId is null)
        {
            return Conflict(new
            {
                message = localizer["SectorNotFoundOrMaterialCodeExists"].Value
            });
        }

        return Created(
            $"/api/materials/{materialId}",
            new
            {
                id = materialId,
                request.SectorId,
                code = request.Code.Trim().ToUpperInvariant(),
                request.Name,
                request.Description
            });
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MaterialDto>>> GetAll(
        [FromQuery] Guid? sectorId,
        CancellationToken cancellationToken)
    {
        var materials = await sender.Send(
            new GetMaterialsQuery(sectorId),
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
