using System.Security.Claims;
using CuttingOptimizer.Api.Contracts.Workshops;
using CuttingOptimizer.Application.Features.ClientQuotes;
using CuttingOptimizer.Application.Features.ClientQuotes.Queries.ListWorkshopQuotes;
using CuttingOptimizer.Application.Abstractions.Entitlements;
using CuttingOptimizer.Application.Features.Dashboard;
using CuttingOptimizer.Application.Features.Dashboard.Queries.GetWorkshopDashboard;
using CuttingOptimizer.Application.Features.Reports;
using CuttingOptimizer.Application.Features.Reports.Queries.GetWorkshopReport;
using CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;
using CuttingOptimizer.Application.Features.Workshops.Commands.DeleteWorkshop;
using CuttingOptimizer.Application.Features.Workshops.Commands.UpdateWorkshop;
using CuttingOptimizer.Application.Features.Workshops.Queries.GetWorkshopById;
using CuttingOptimizer.Application.Features.Workshops.Queries.GetWorkshops;
using CuttingOptimizer.Application.Resources;
using CuttingOptimizer.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class WorkshopsController(
    ISender sender,
    IPlanEntitlementService entitlements,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkshopRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateWorkshopCommand(request.Name);

        var workshopId = await sender.Send(command, cancellationToken);

        return Created($"/api/workshops/{workshopId}", new
        {
            id = workshopId
        });
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var workshop = await sender.Send(
            new GetWorkshopByIdQuery(id),
            cancellationToken);

        if (workshop is null)
            return NotFound();

        return Ok(workshop);
    }

    [HttpGet("{workshopId:guid}/dashboard")]
    public async Task<ActionResult<WorkshopDashboardDto>> GetDashboard(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var dashboard = await sender.Send(
            new GetWorkshopDashboardQuery(workshopId),
            cancellationToken);

        if (dashboard is null)
        {
            return NotFound(new
            {
                message = localizer["WorkshopDashboardNotFound"].Value
            });
        }

        return Ok(dashboard);
    }

    [HttpGet("{workshopId:guid}/quotes")]
    public async Task<ActionResult<IReadOnlyList<ClientQuoteSummaryDto>>> GetQuotes(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        if (!await entitlements.HasAsync(
                workshopId,
                PlanFeatureCodes.ClientQuotes,
                cancellationToken))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Client quotes are not enabled for this plan."
            });
        }

        var quotes = await sender.Send(
            new ListWorkshopQuotesQuery(workshopId),
            cancellationToken);

        if (quotes is null)
        {
            return NotFound(new
            {
                message = localizer["WorkshopNotFound"].Value
            });
        }

        return Ok(quotes);
    }

    [HttpGet("{workshopId:guid}/reports/summary")]
    public async Task<ActionResult<WorkshopReportDto>> GetReportSummary(
        Guid workshopId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        if (!await entitlements.HasAsync(
                workshopId,
                PlanFeatureCodes.WorkshopReports,
                cancellationToken))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Workshop reports are not enabled for this plan."
            });
        }

        var toUtc = to?.ToUniversalTime() ?? DateTime.UtcNow;
        var fromUtc = from?.ToUniversalTime() ?? toUtc.AddDays(-30);

        var report = await sender.Send(
            new GetWorkshopReportQuery(workshopId, fromUtc, toUtc),
            cancellationToken);

        if (report is null)
        {
            return NotFound(new
            {
                message = localizer["WorkshopNotFound"].Value
            });
        }

        return Ok(report);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var isAdmin = User.IsInRole(AppRoles.Admin);
        Guid? restrict = null;
        if (!isAdmin)
        {
            var claim = User.FindFirstValue("workshop_id");
            if (Guid.TryParse(claim, out var workshopId))
                restrict = workshopId;
        }

        var result = await sender.Send(
            new GetWorkshopsQuery(page, pageSize, restrict, isAdmin),
            cancellationToken);

        return Ok(result);
    }
    
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateWorkshopRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdateWorkshopCommand(
                id,
                request.Name,
                request.ProductionPricePerM2),
            cancellationToken);

        if (!updated)
            return NotFound();

        return NoContent();
    }
    
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(
            new DeleteWorkshopCommand(id),
            cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}