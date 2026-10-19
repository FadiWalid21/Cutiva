namespace CuttingOptimizer.Application.Abstractions.Auth;

public interface IPasswordResetService
{
    /// <summary>Always succeeds from the caller's perspective (no email enumeration).</summary>
    Task RequestForgotPasswordAsync(
        string email,
        bool arabic,
        CancellationToken cancellationToken);

    Task<PasswordResetOutcome> ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        CancellationToken cancellationToken);
}

public enum PasswordResetOutcome
{
    Succeeded,
    InvalidOrExpired,
    WeakPassword
}
