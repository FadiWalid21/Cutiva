using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Domain.Constants;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;
using CuttingOptimizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Infrastructure.Auth;

public sealed class IdentityAuthService(
    UserManager<ApplicationUser> userManager,
    JwtTokenService jwtTokenService,
    IApplicationDbContext dbContext)
    : IAuthService
{
    public async Task<AuthResult> RegisterAsync(
        string email,
        string password,
        string? displayName,
        CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            throw new AuthDuplicateEmailException();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(user, password);
        if (!create.Succeeded)
        {
            throw new AuthOperationException(
                create.Errors.Select(error => error.Description).ToList());
        }

        await userManager.AddToRoleAsync(user, AppRoles.WorkshopOwner);

        var workshop = new Workshop
        {
            Name = ResolveWorkshopName(displayName, email)
        };
        dbContext.Workshops.Add(workshop);

        var starter = await dbContext.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Code == SubscriptionPlanCodes.Starter && p.IsActive,
                cancellationToken);

        if (starter is not null)
        {
            var now = DateTime.UtcNow;
            dbContext.WorkshopSubscriptions.Add(new WorkshopSubscription
            {
                WorkshopId = workshop.Id,
                PlanId = starter.Id,
                StartsAtUtc = now,
                EndsAtUtc = now.AddDays(starter.DurationDays > 0 ? starter.DurationDays : 30),
                Status = SubscriptionStatus.Active
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        user.WorkshopId = workshop.Id;
        await userManager.UpdateAsync(user);

        return await BuildAuthResultAsync(user, cancellationToken);
    }

    public async Task<AuthResult?> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        if (await userManager.IsLockedOutAsync(user))
            return null;

        var valid = await userManager.CheckPasswordAsync(user, password);
        if (!valid)
            return null;

        return await BuildAuthResultAsync(user, cancellationToken);
    }

    public async Task<AuthUserDto?> GetMeAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return null;

        var roles = await userManager.GetRolesAsync(user);
        var subscription = await LoadSubscriptionAsync(user.WorkshopId, cancellationToken);
        if (subscription is not null && user.WorkshopId.HasValue)
        {
            var seats = await userManager.Users
                .CountAsync(u => u.WorkshopId == user.WorkshopId, cancellationToken);
            subscription = subscription with { SeatUsed = seats };
        }

        return new AuthUserDto(
            user.Id,
            user.Email!,
            user.DisplayName,
            roles.ToList(),
            user.WorkshopId,
            subscription);
    }

    private async Task<AuthResult> BuildAuthResultAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (token, expires) = jwtTokenService.CreateToken(user, roles);
        var subscription = await LoadSubscriptionAsync(user.WorkshopId, cancellationToken);
        if (subscription is not null && user.WorkshopId.HasValue)
        {
            var seats = await userManager.Users
                .CountAsync(u => u.WorkshopId == user.WorkshopId, cancellationToken);
            subscription = subscription with { SeatUsed = seats };
        }

        return new AuthResult(
            token,
            expires,
            user.Email!,
            user.DisplayName,
            user.Id,
            roles.ToList(),
            user.WorkshopId,
            subscription);
    }

    private async Task<SubscriptionSummaryDto?> LoadSubscriptionAsync(
        Guid? workshopId,
        CancellationToken cancellationToken)
    {
        if (!workshopId.HasValue)
            return null;

        var now = DateTime.UtcNow;
        var sub = await dbContext.WorkshopSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.WorkshopId == workshopId.Value)
            .OrderByDescending(s => s.EndsAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return SubscriptionSummaryFactory.FromCurrent(sub, now);
    }

    private static string ResolveWorkshopName(string? displayName, string email)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName.Trim();

        var at = email.IndexOf('@');
        var local = at > 0 ? email[..at] : email;
        return string.IsNullOrWhiteSpace(local) ? "Workshop" : local.Trim();
    }
}
