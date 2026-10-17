using CuttingOptimizer.Api.Contracts.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;
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
}
