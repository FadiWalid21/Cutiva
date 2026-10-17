using CuttingOptimizer.Api.Contracts.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans;
using CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/cutting-plans")]
public sealed class CuttingPlansController(
    ISender sender) : ControllerBase
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
                message = "Material was not found, or one or more required cuts were missing or do not belong to the material."
            });
        }

        return Ok(plan);
    }
}
