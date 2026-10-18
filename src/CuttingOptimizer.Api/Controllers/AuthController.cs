using System.Security.Claims;
using CuttingOptimizer.Api.Contracts.Auth;
using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Application.Features.Auth;
using CuttingOptimizer.Application.Features.Auth.Commands.Login;
using CuttingOptimizer.Application.Features.Auth.Commands.Register;
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
    IStringLocalizer<Messages> localizer) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterCommand(
                request.Email,
                request.Password,
                request.DisplayName),
            cancellationToken);

        return result switch
        {
            RegisterSucceeded succeeded => Created(
                "/api/auth/me",
                succeeded.Token),
            RegisterDuplicateEmail => Conflict(new
            {
                message = localizer["AuthEmailExists"].Value
            }),
            RegisterFailed failed => BadRequest(new
            {
                message = localizer["AuthRegisterFailed"].Value,
                errors = new { password = failed.Errors.ToArray() }
            }),
            _ => StatusCode(500)
        };
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

        return Ok(new AuthMeResponse(me.UserId, me.Email, me.DisplayName));
    }
}
