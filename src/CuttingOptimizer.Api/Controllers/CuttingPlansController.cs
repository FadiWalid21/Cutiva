using CuttingOptimizer.Api.Contracts.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans.Commands.ApproveCuttingPlan;
using CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;
using CuttingOptimizer.Application.Features.CuttingPlans.Queries.GetCuttingPlanById;
using CuttingOptimizer.Application.Features.CuttingPlans.Queries.GetCuttingPlanComparison;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/cutting-plans")]
public sealed class CuttingPlansController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CuttingPlanComparisonDto>> Generate(
        [FromBody] GenerateCuttingPlanRequest request,
        CancellationToken cancellationToken)
    {
        var comparison = await sender.Send(
            new GenerateCuttingPlanQuery(
                request.MaterialId,
                request.KerfMm!.Value,
                request.RequiredCutIds,
                request.UseRemnants),
            cancellationToken);

        if (comparison is null)
        {
            return NotFound(new
            {
                message = localizer["CuttingPlanNotFound"].Value
            });
        }

        return Ok(comparison);
    }

    [HttpGet("comparisons/{groupId:guid}")]
    public async Task<ActionResult<CuttingPlanComparisonDto>> GetComparison(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var comparison = await sender.Send(
            new GetCuttingPlanComparisonQuery(groupId),
            cancellationToken);

        if (comparison is null)
        {
            return NotFound(new
            {
                message = localizer["CuttingPlanByIdNotFound"].Value
            });
        }

        return Ok(comparison);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CuttingPlanDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var plan = await sender.Send(
            new GetCuttingPlanByIdQuery(id),
            cancellationToken);

        if (plan is null)
        {
            return NotFound(new
            {
                message = localizer["CuttingPlanByIdNotFound"].Value
            });
        }

        return Ok(plan);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<CuttingPlanDto>> Approve(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ApproveCuttingPlanCommand(id),
            cancellationToken);

        return result switch
        {
            ApproveCuttingPlanSucceeded succeeded => Ok(succeeded.Plan),
            ApproveCuttingPlanNotFound => NotFound(new
            {
                message = localizer["CuttingPlanByIdNotFound"].Value
            }),
            ApproveCuttingPlanAlreadyApproved => Conflict(new
            {
                message = localizer["CuttingPlanAlreadyApproved"].Value
            }),
            ApproveCuttingPlanIncomplete => BadRequest(new
            {
                message = localizer["CuttingPlanIncomplete"].Value
            }),
            _ => StatusCode(500)
        };
    }
}
