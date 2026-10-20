using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Application.Abstractions.Email;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Infrastructure.Email;
using CuttingOptimizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Infrastructure.Auth;

public sealed class PasswordResetService(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext db,
    IEmailSender emailSender,
    ILogger<PasswordResetService> logger)
    : IPasswordResetService
{
    private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
    private const int MaxAttempts = 5;

    public async Task<ForgotPasswordOutcome> RequestForgotPasswordAsync(
        string email,
        bool arabic,
        CancellationToken cancellationToken)
    {
        var normalized = email.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return ForgotPasswordOutcome.EmailNotFound;

        var user = await userManager.FindByEmailAsync(normalized);
        if (user is null)
            return ForgotPasswordOutcome.EmailNotFound;

        var code = AuthEmailTemplates.GenerateOtp();
        var hash = AuthEmailTemplates.HashOtp(code);
        var now = DateTime.UtcNow;

        var pending = await db.PasswordResetOtps
            .Where(x => x.UserId == user.Id && x.ConsumedAtUtc == null)
            .ToListAsync(cancellationToken);
        foreach (var row in pending)
            row.ConsumedAtUtc = now;

        db.PasswordResetOtps.Add(new PasswordResetOtp
        {
            UserId = user.Id,
            CodeHash = hash,
            ExpiresAtUtc = now.Add(OtpLifetime),
            CreatedAtUtc = now
        });
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendAsync(
                new EmailMessage(
                    user.Email!,
                    user.DisplayName,
                    AuthEmailTemplates.ResetSubject(arabic),
                    AuthEmailTemplates.ResetOtpHtml(code, arabic)),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password reset OTP to {Email}", user.Email);
        }

        return ForgotPasswordOutcome.Sent;
    }

    public async Task<PasswordResetOutcome> ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
            return PasswordResetOutcome.InvalidOrExpired;

        var now = DateTime.UtcNow;
        var otp = await db.PasswordResetOtps
            .Where(x =>
                x.UserId == user.Id
                && x.ConsumedAtUtc == null
                && x.ExpiresAtUtc >= now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null || otp.AttemptCount >= MaxAttempts)
            return PasswordResetOutcome.InvalidOrExpired;

        var hash = AuthEmailTemplates.HashOtp(code);
        if (!string.Equals(hash, otp.CodeHash, StringComparison.OrdinalIgnoreCase))
        {
            otp.AttemptCount++;
            await db.SaveChangesAsync(cancellationToken);
            return PasswordResetOutcome.InvalidOrExpired;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return PasswordResetOutcome.WeakPassword;

        otp.ConsumedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        return PasswordResetOutcome.Succeeded;
    }

    public async Task<ChangePasswordOutcome> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword,
        bool arabic,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return ChangePasswordOutcome.UserNotFound;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return ChangePasswordOutcome.UserNotFound;

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            return ChangePasswordOutcome.WeakPassword;

        if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
            return ChangePasswordOutcome.SameAsCurrent;

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e =>
                    string.Equals(e.Code, "PasswordMismatch", StringComparison.OrdinalIgnoreCase)))
                return ChangePasswordOutcome.WrongCurrentPassword;

            return ChangePasswordOutcome.WeakPassword;
        }

        try
        {
            await emailSender.SendAsync(
                new EmailMessage(
                    user.Email!,
                    user.DisplayName,
                    AuthEmailTemplates.PasswordChangedSubject(arabic),
                    AuthEmailTemplates.PasswordChangedHtml(user.DisplayName, arabic)),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send password-changed alert to {Email}", user.Email);
        }

        return ChangePasswordOutcome.Succeeded;
    }
}
