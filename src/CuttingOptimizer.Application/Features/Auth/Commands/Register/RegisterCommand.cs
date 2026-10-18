using CuttingOptimizer.Application.Features.Auth;
using MediatR;

namespace CuttingOptimizer.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string? DisplayName)
    : IRequest<RegisterResult>;

public abstract record RegisterResult;

public sealed record RegisterSucceeded(AuthTokenResponse Token) : RegisterResult;

public sealed record RegisterDuplicateEmail : RegisterResult;

public sealed record RegisterFailed(IReadOnlyList<string> Errors) : RegisterResult;
