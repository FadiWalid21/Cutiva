using CuttingOptimizer.Api.Contracts.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans.Commands.ApproveCuttingPlan;
using CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;
using CuttingOptimizer.Application.Features.CuttingPlans.Queries.GetCuttingPlanById;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/cutting-plans")]
public sealed class CuttingPlansController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CuttingPlanDto>> Generate(
        [FromBody] GenerateCuttingPlanRequest request,
        CancellationToken cancellationToken)
    {
        var plan = await sender.Send(
            new GenerateCuttingPlanQuery(
                request.MaterialId,
                request.KerfMm!.Value,
                request.RequiredCutIds),
            cancellationToken);

        if (plan is null)
        {
            return NotFound(new
            {
                message = localizer["CuttingPlanNotFound"].Value
            });
        }

        return Ok(plan);
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
            ApproveCuttingPlanInsufficientStock => BadRequest(new
            {
                message = localizer["CuttingPlanInsufficientStock"].Value
            }),
            _ => StatusCode(500)
        };
    }
}
