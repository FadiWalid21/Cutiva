using System.Globalization;
using CuttingOptimizer.Application.Abstractions.Admin;
using CuttingOptimizer.Application.Features.Admin;
using CuttingOptimizer.Domain.Constants;
using CuttingOptimizer.Infrastructure.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[ApiController]
[Route("api/admin")]
public sealed class AdminController(IAdminClientService admin) : ControllerBase
{
    [HttpGet("clients")]
    public async Task<ActionResult<IReadOnlyList<AdminClientListItemDto>>> ListClients(
        CancellationToken cancellationToken)
    {
        return Ok(await admin.ListClientsAsync(cancellationToken));
    }

    [HttpGet("clients/{workshopId:guid}")]
    public async Task<ActionResult<AdminClientDetailDto>> GetClient(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var client = await admin.GetClientAsync(workshopId, cancellationToken);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost("clients")]
    public async Task<ActionResult<CreateAdminClientResult>> CreateClient(
        [FromBody] CreateAdminClientRequest request,
        CancellationToken cancellationToken)
    {
        var created = await admin.CreateClientAsync(
            request,
            IsArabicRequest(),
            cancellationToken);
        if (created is null)
            return BadRequest(new { message = "Could not create client (email or plan invalid)." });

        return Created(
            $"/api/admin/clients/{created.Client.WorkshopId}",
            created);
    }

    [HttpPut("clients/{workshopId:guid}")]
    public async Task<ActionResult<AdminClientDetailDto>> UpdateClient(
        Guid workshopId,
        [FromBody] UpdateAdminClientRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await admin.UpdateClientAsync(workshopId, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPut("clients/{workshopId:guid}/subscription")]
    public async Task<ActionResult<AdminClientDetailDto>> UpsertSubscription(
        Guid workshopId,
        [FromBody] UpsertSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await admin.UpsertSubscriptionAsync(
            workshopId,
            request,
            cancellationToken);
        if (updated is null)
            return Conflict(new { message = "Plan change rejected (invalid plan or seat limit)." });
        return Ok(updated);
    }

    [HttpDelete("clients/{workshopId:guid}/subscription")]
    public async Task<ActionResult<AdminClientDetailDto>> SuspendSubscription(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var updated = await admin.SuspendSubscriptionAsync(workshopId, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpGet("clients/{workshopId:guid}/users")]
    public async Task<ActionResult<IReadOnlyList<AdminWorkshopUserDto>>> ListUsers(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var client = await admin.GetClientAsync(workshopId, cancellationToken);
        if (client is null)
            return NotFound();
        return Ok(await admin.ListUsersAsync(workshopId, cancellationToken));
    }

    [HttpPost("clients/{workshopId:guid}/users")]
    public async Task<ActionResult<AddWorkshopUserResult>> AddUser(
        Guid workshopId,
        [FromBody] AddWorkshopUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await admin.AddUserAsync(
                workshopId,
                request,
                IsArabicRequest(),
                cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }
        catch (AdminSeatException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("clients/{workshopId:guid}/users/{userId}/reset-password")]
    public async Task<ActionResult<AdminTempPasswordResult>> ResetUserPassword(
        Guid workshopId,
        string userId,
        [FromBody] ResetPasswordRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await admin.ResetUserPasswordAsync(
            workshopId,
            userId,
            request?.NewPassword,
            IsArabicRequest(),
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("clients/{workshopId:guid}/users/{userId}/lock")]
    public async Task<IActionResult> LockUser(
        Guid workshopId,
        string userId,
        CancellationToken cancellationToken)
    {
        var ok = await admin.SetUserLockoutAsync(workshopId, userId, true, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("clients/{workshopId:guid}/users/{userId}/unlock")]
    public async Task<IActionResult> UnlockUser(
        Guid workshopId,
        string userId,
        CancellationToken cancellationToken)
    {
        var ok = await admin.SetUserLockoutAsync(workshopId, userId, false, cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<AdminPlanDto>>> ListPlans(
        CancellationToken cancellationToken)
    {
        return Ok(await admin.ListPlansAsync(cancellationToken));
    }

    [HttpPut("plans/{planId:guid}")]
    public async Task<ActionResult<AdminPlanDto>> UpdatePlan(
        Guid planId,
        [FromBody] UpdateSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await admin.UpdatePlanAsync(planId, request, cancellationToken);
        if (updated is null)
            return BadRequest(new { message = "Invalid plan update." });
        return Ok(updated);
    }

    [HttpGet("reports/summary")]
    public async Task<ActionResult<AdminReportSummaryDto>> ReportSummary(
        CancellationToken cancellationToken)
    {
        return Ok(await admin.GetReportSummaryAsync(cancellationToken));
    }

    private static bool IsArabicRequest()
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return string.Equals(culture, "ar", StringComparison.OrdinalIgnoreCase);
    }
}
