namespace CuttingOptimizer.Api.Contracts.Auth;

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordWithCodeRequest(
    string Email,
    string Code,
    string NewPassword);
