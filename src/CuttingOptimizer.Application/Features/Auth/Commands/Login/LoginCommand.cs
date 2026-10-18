using CuttingOptimizer.Application.Features.Auth;
using MediatR;

namespace CuttingOptimizer.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<LoginResult>;

public abstract record LoginResult;

public sealed record LoginSucceeded(AuthTokenResponse Token) : LoginResult;

public sealed record LoginInvalidCredentials : LoginResult;
