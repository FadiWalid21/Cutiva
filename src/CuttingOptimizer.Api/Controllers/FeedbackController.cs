using System.Security.Claims;
using CuttingOptimizer.Api.Contracts.Feedback;
using CuttingOptimizer.Application.Features.Feedback;
using CuttingOptimizer.Application.Features.Feedback.Commands.SubmitFeedback;
using CuttingOptimizer.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/feedback")]
public sealed class FeedbackController(
    ISender sender,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<WorkshopFeedbackDto>> Submit(
        [FromBody] SubmitFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Unauthorized();

        var ownerName = string.IsNullOrWhiteSpace(user.DisplayName)
            ? (user.Email ?? "User")
            : user.DisplayName.Trim();

        var result = await sender.Send(
            new SubmitFeedbackCommand(
                userId,
                ownerName,
                request.WorkshopId,
                request.Rating,
                request.Comment,
                request.Trigger),
            cancellationToken);

        return result switch
        {
            SubmitFeedbackSucceeded ok => Ok(ok.Feedback),
            SubmitFeedbackWorkshopNotFound => NotFound(new
            {
                message = "Workshop not found."
            }),
            _ => BadRequest()
        };
    }
}
