using CuttingOptimizer.Api.Contracts.ClientQuotes;
using CuttingOptimizer.Application.Features.ClientQuotes;
using CuttingOptimizer.Application.Features.ClientQuotes.Commands.UpsertClientQuote;
using CuttingOptimizer.Application.Features.ClientQuotes.Queries.GetClientQuote;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/quotes")]
public sealed class ClientQuotesController(
    ISender sender,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertClientQuoteRequest request,
        CancellationToken cancellationToken)
    {
        var surcharges = (request.Surcharges ?? [])
            .Select(s => new ClientQuoteSurchargeInput(s.Reason, s.Amount))
            .ToList();

        var result = await sender.Send(
            new UpsertClientQuoteCommand(
                request.ProjectId,
                request.CuttingPlanId,
                request.Notes,
                surcharges),
            cancellationToken);

        return result switch
        {
            UpsertClientQuoteSucceeded ok => Ok(ok.Quote),
            UpsertClientQuotePlanNotApproved => Conflict(new
            {
                message = localizer["QuotePlanNotApproved"].Value
            }),
            UpsertClientQuoteNoBillableLines => BadRequest(new
            {
                message = localizer["QuoteNoBillableLines"].Value
            }),
            UpsertClientQuoteFeatureDisabled => StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "Client quotes are not enabled for this plan." }),
            _ => NotFound(new
            {
                message = localizer["QuoteNotFound"].Value
            })
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientQuoteDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var quote = await sender.Send(
            new GetClientQuoteQuery(id),
            cancellationToken);

        if (quote is null)
        {
            return NotFound(new
            {
                message = localizer["QuoteNotFound"].Value
            });
        }

        return Ok(quote);
    }

    [HttpGet]
    public async Task<ActionResult<ClientQuoteDto>> GetByPlan(
        [FromQuery] Guid projectId,
        [FromQuery] Guid cuttingPlanId,
        CancellationToken cancellationToken)
    {
        var quote = await sender.Send(
            new GetClientQuoteByPlanQuery(projectId, cuttingPlanId),
            cancellationToken);

        if (quote is null)
        {
            return NotFound(new
            {
                message = localizer["QuoteNotFound"].Value
            });
        }

        return Ok(quote);
    }
}
