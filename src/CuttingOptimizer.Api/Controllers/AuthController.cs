using System.Globalization;
using System.Security.Claims;
using CuttingOptimizer.Api.Contracts.Auth;
using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Application.Features.Auth;
using CuttingOptimizer.Application.Features.Auth.Commands.Login;
using CuttingOptimizer.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    ISender sender,
    IAuthService authService,
    IPasswordResetService passwordResetService,
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register()
    {
        return StatusCode(StatusCodes.Status410Gone, new
        {
            message = "Public registration is disabled. Contact your administrator."
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return result switch
        {
            LoginSucceeded succeeded => Ok(succeeded.Token),
            LoginInvalidCredentials => Unauthorized(new
            {
                message = localizer["AuthInvalidCredentials"].Value
            }),
            _ => StatusCode(500)
        };
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var arabic = IsArabicRequest();
        await passwordResetService.RequestForgotPasswordAsync(
            request.Email ?? string.Empty,
            arabic,
            cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordWithCodeRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = await passwordResetService.ResetPasswordAsync(
            request.Email ?? string.Empty,
            request.Code ?? string.Empty,
            request.NewPassword ?? string.Empty,
            cancellationToken);

        return outcome switch
        {
            PasswordResetOutcome.Succeeded => NoContent(),
            PasswordResetOutcome.WeakPassword => BadRequest(new
            {
                message = localizer["AuthRegisterFailed"].Value
            }),
            _ => BadRequest(new
            {
                message = "Invalid or expired code."
            })
        };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthMeResponse>> Me(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var me = await authService.GetMeAsync(userId, cancellationToken);
        if (me is null)
            return Unauthorized();

        return Ok(new AuthMeResponse(
            me.UserId,
            me.Email,
            me.DisplayName,
            me.Roles,
            me.WorkshopId,
            me.Subscription,
            me.Features));
    }

    private bool IsArabicRequest()
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return string.Equals(culture, "ar", StringComparison.OrdinalIgnoreCase);
    }
}
