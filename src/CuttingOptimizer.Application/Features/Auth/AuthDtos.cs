namespace CuttingOptimizer.Application.Features.Auth;

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Email,
    string? DisplayName);

public sealed record AuthMeResponse(
    string UserId,
    string Email,
    string? DisplayName);
