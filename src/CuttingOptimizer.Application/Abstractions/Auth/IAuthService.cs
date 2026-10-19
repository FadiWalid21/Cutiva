namespace CuttingOptimizer.Application.Abstractions.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(
        string email,
        string password,
        string? displayName,
        CancellationToken cancellationToken);

    Task<AuthResult?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<AuthUserDto?> GetMeAsync(
        string userId,
        CancellationToken cancellationToken);
}

public sealed record AuthResult(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Email,
    string? DisplayName,
    string UserId,
    IReadOnlyList<string> Roles,
    Guid? WorkshopId,
    SubscriptionSummaryDto? Subscription);

public sealed record AuthUserDto(
    string UserId,
    string Email,
    string? DisplayName,
    IReadOnlyList<string> Roles,
    Guid? WorkshopId,
    SubscriptionSummaryDto? Subscription);

public sealed record SubscriptionSummaryDto(
    string PlanCode,
    string PlanName,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string Status,
    int DaysRemaining,
    int? MaxUsers = null,
    int? SeatUsed = null);

public enum AuthFailureKind
{
    None,
    DuplicateEmail,
    InvalidCredentials,
    Validation
}
