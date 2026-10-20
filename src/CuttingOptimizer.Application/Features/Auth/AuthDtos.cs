using CuttingOptimizer.Application.Abstractions.Auth;

namespace CuttingOptimizer.Application.Features.Auth;

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Email,
    string? DisplayName,
    IReadOnlyList<string> Roles,
    Guid? WorkshopId,
    SubscriptionSummaryDto? Subscription,
    IReadOnlyList<string> Features);

public sealed record AuthMeResponse(
    string UserId,
    string Email,
    string? DisplayName,
    IReadOnlyList<string> Roles,
    Guid? WorkshopId,
    SubscriptionSummaryDto? Subscription,
    IReadOnlyList<string> Features);
