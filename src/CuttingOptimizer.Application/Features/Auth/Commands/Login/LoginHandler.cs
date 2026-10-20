using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Application.Features.Auth;
using MediatR;

namespace CuttingOptimizer.Application.Features.Auth.Commands.Login;

public sealed class LoginHandler(IAuthService authService)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);

        if (result is null)
            return new LoginInvalidCredentials();

        return new LoginSucceeded(new AuthTokenResponse(
            result.AccessToken,
            result.ExpiresAtUtc,
            result.Email,
            result.DisplayName,
            result.Roles,
            result.WorkshopId,
            result.Subscription,
            result.Features));
    }
}
