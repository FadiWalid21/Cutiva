using CuttingOptimizer.Application.Abstractions.Pricing;
using CuttingOptimizer.Application.Features.Pricing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/pricing")]
public sealed class PricingController(IPricingDisplayService pricing) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("display")]
    public async Task<ActionResult<PricingDisplayDto>> Display(
        CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(ip)
            && HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            ip = forwarded.ToString();
        }

        return Ok(await pricing.GetDisplayAsync(ip, cancellationToken));
    }
}
