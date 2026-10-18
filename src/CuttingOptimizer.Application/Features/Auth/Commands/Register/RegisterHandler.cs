using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Application.Features.Auth;
using MediatR;

namespace CuttingOptimizer.Application.Features.Auth.Commands.Register;

public sealed class RegisterHandler(IAuthService authService)
    : IRequestHandler<RegisterCommand, RegisterResult>
{
    public async Task<RegisterResult> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.RegisterAsync(
                request.Email.Trim(),
                request.Password,
                string.IsNullOrWhiteSpace(request.DisplayName)
                    ? null
                    : request.DisplayName.Trim(),
                cancellationToken);

            return new RegisterSucceeded(new AuthTokenResponse(
                result.AccessToken,
                result.ExpiresAtUtc,
                result.Email,
                result.DisplayName));
        }
        catch (AuthDuplicateEmailException)
        {
            return new RegisterDuplicateEmail();
        }
        catch (AuthOperationException ex)
        {
            return new RegisterFailed(ex.Errors);
        }
    }
}
