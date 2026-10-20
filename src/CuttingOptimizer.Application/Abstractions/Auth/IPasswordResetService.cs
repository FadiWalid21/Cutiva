namespace CuttingOptimizer.Application.Abstractions.Auth;

public interface IPasswordResetService
{
    Task<ForgotPasswordOutcome> RequestForgotPasswordAsync(
        string email,
        bool arabic,
        CancellationToken cancellationToken);

    Task<PasswordResetOutcome> ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        CancellationToken cancellationToken);
}

public enum ForgotPasswordOutcome
{
    Sent,
    EmailNotFound
}

public enum PasswordResetOutcome
{
    Succeeded,
    InvalidOrExpired,
    WeakPassword
}
