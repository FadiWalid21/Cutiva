using CuttingOptimizer.Application.Abstractions.Admin;
using CuttingOptimizer.Application.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/plans")]
public sealed class PlansController(IAdminClientService admin) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublicPlanDto>>> List(
        CancellationToken cancellationToken)
    {
        return Ok(await admin.ListPublicPlansAsync(cancellationToken));
    }
}
