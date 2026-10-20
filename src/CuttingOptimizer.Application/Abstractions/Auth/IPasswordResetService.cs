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

    Task<ChangePasswordOutcome> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword,
        bool arabic,
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

public enum ChangePasswordOutcome
{
    Succeeded,
    UserNotFound,
    WrongCurrentPassword,
    WeakPassword,
    SameAsCurrent
}
