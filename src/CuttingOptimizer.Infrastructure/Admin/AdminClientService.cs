using CuttingOptimizer.Application.Abstractions.Admin;
using CuttingOptimizer.Application.Abstractions.Email;
using CuttingOptimizer.Application.Features.Admin;
using CuttingOptimizer.Domain.Constants;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;
using CuttingOptimizer.Infrastructure.Auth;
using CuttingOptimizer.Infrastructure.Email;
using CuttingOptimizer.Infrastructure.Identity;
using CuttingOptimizer.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Infrastructure.Admin;

public sealed class AdminClientService(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    ILogger<AdminClientService> logger)
    : IAdminClientService
{
    public async Task<IReadOnlyList<AdminClientListItemDto>> ListClientsAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var workshops = await db.Workshops
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .Select(w => new
            {
                w.Id,
                w.Name,
                ProjectCount = w.Projects.Count
            })
            .ToListAsync(cancellationToken);

        var owners = await userManager.Users
            .AsNoTracking()
            .Where(u => u.WorkshopId != null)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.DisplayName,
                u.WorkshopId,
                u.IsPrimaryContact
            })
            .ToListAsync(cancellationToken);

        var seatCounts = owners
            .Where(o => o.WorkshopId.HasValue)
            .GroupBy(o => o.WorkshopId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var ownerByWorkshop = owners
            .Where(o => o.WorkshopId.HasValue)
            .GroupBy(o => o.WorkshopId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.IsPrimaryContact).First());

        var workshopIds = workshops.Select(w => w.Id).ToList();
        var subs = await db.WorkshopSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => workshopIds.Contains(s.WorkshopId))
            .ToListAsync(cancellationToken);

        var subByWorkshop = subs
            .GroupBy(s => s.WorkshopId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.EndsAtUtc).First());

        return workshops.Select(w =>
        {
            ownerByWorkshop.TryGetValue(w.Id, out var owner);
            subByWorkshop.TryGetValue(w.Id, out var sub);
            seatCounts.TryGetValue(w.Id, out var seats);
            var summary = SubscriptionSummaryFactory.FromCurrent(sub, now);
            return new AdminClientListItemDto(
                w.Id,
                w.Name,
                owner?.Id,
                owner?.Email,
                owner?.DisplayName,
                summary?.PlanCode,
                summary?.PlanName,
                sub?.Plan.MaxUsers,
                seats,
                summary?.Status,
                sub?.PaymentStatus.ToString(),
                summary?.EndsAtUtc,
                summary?.DaysRemaining,
                w.ProjectCount);
        }).ToList();
    }

    public async Task<AdminClientDetailDto?> GetClientAsync(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var workshop = await db.Workshops
            .AsNoTracking()
            .Where(w => w.Id == workshopId)
            .Select(w => new
            {
                w.Id,
                w.Name,
                w.ProductionPricePerM2,
                ProjectCount = w.Projects.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (workshop is null)
            return null;

        var quoteCount = await db.ClientQuotes
            .AsNoTracking()
            .CountAsync(q => q.Project.WorkshopId == workshopId, cancellationToken);

        var users = await ListUsersAsync(workshopId, cancellationToken);
        var owner = users.FirstOrDefault(u => u.IsPrimaryContact) ?? users.FirstOrDefault();

        var sub = await db.WorkshopSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.WorkshopId == workshopId)
            .OrderByDescending(s => s.EndsAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var summary = SubscriptionSummaryFactory.FromCurrent(sub, DateTime.UtcNow);

        return new AdminClientDetailDto(
            workshop.Id,
            workshop.Name,
            workshop.ProductionPricePerM2,
            owner?.UserId,
            owner?.Email,
            owner?.DisplayName,
            summary?.PlanCode,
            summary?.PlanName,
            sub?.Plan.MaxUsers,
            users.Count,
            summary?.Status,
            sub?.PaymentStatus.ToString(),
            sub?.PaymentAmount,
            sub?.PaymentCurrency,
            sub?.PaymentNote,
            sub?.PaymentConfirmedAtUtc,
            summary?.StartsAtUtc,
            summary?.EndsAtUtc,
            summary?.DaysRemaining,
            workshop.ProjectCount,
            quoteCount,
            users);
    }

    public async Task<CreateAdminClientResult?> CreateClientAsync(
        CreateAdminClientRequest request,
        bool arabic,
        CancellationToken cancellationToken)
    {
        var email = request.OwnerEmail.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            return null;

        var plan = await ResolvePlanAsync(request.PlanCode, cancellationToken);
        if (plan is null)
            return null;

        var tempPassword = string.IsNullOrWhiteSpace(request.OwnerPassword)
            ? AuthEmailTemplates.GenerateTemporaryPassword()
            : request.OwnerPassword;

        var workshop = new Workshop { Name = request.WorkshopName.Trim() };
        db.Workshops.Add(workshop);

        var duration = request.DurationDays is > 0
            ? request.DurationDays.Value
            : plan.DurationDays;
        var now = DateTime.UtcNow;
        var paymentStatus = request.PaymentStatus ?? PaymentStatus.Pending;
        db.WorkshopSubscriptions.Add(new WorkshopSubscription
        {
            WorkshopId = workshop.Id,
            PlanId = plan.Id,
            StartsAtUtc = now,
            EndsAtUtc = now.AddDays(duration),
            Status = SubscriptionStatus.Active,
            PaymentStatus = paymentStatus,
            PaymentAmount = request.PaymentAmount ?? plan.PriceSar,
            PaymentCurrency = string.IsNullOrWhiteSpace(request.PaymentCurrency)
                ? "SAR"
                : request.PaymentCurrency.Trim().ToUpperInvariant(),
            PaymentNote = request.PaymentNote,
            PaymentConfirmedAtUtc = paymentStatus is PaymentStatus.Confirmed or PaymentStatus.Waived
                ? now
                : null
        });

        await db.SaveChangesAsync(cancellationToken);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(request.OwnerDisplayName)
                ? null
                : request.OwnerDisplayName.Trim(),
            EmailConfirmed = true,
            WorkshopId = workshop.Id,
            IsPrimaryContact = true
        };

        var created = await userManager.CreateAsync(user, tempPassword);
        if (!created.Succeeded)
        {
            db.Workshops.Remove(workshop);
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }

        await userManager.AddToRoleAsync(user, AppRoles.WorkshopOwner);
        var emailSent = await TrySendWelcomeAsync(
            user,
            tempPassword,
            arabic,
            cancellationToken);
        var detail = await GetClientAsync(workshop.Id, cancellationToken);
        if (detail is null)
            return null;

        return new CreateAdminClientResult(detail, tempPassword, emailSent);
    }

    public async Task<AdminClientDetailDto?> UpdateClientAsync(
        Guid workshopId,
        UpdateAdminClientRequest request,
        CancellationToken cancellationToken)
    {
        var workshop = await db.Workshops
            .FirstOrDefaultAsync(w => w.Id == workshopId, cancellationToken);
        if (workshop is null)
            return null;

        workshop.Name = request.WorkshopName.Trim();

        var owner = await userManager.Users
            .FirstOrDefaultAsync(
                u => u.WorkshopId == workshopId && u.IsPrimaryContact,
                cancellationToken)
            ?? await userManager.Users
                .FirstOrDefaultAsync(u => u.WorkshopId == workshopId, cancellationToken);

        if (owner is not null && request.OwnerDisplayName is not null)
        {
            owner.DisplayName = string.IsNullOrWhiteSpace(request.OwnerDisplayName)
                ? null
                : request.OwnerDisplayName.Trim();
            await userManager.UpdateAsync(owner);
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetClientAsync(workshopId, cancellationToken);
    }

    public async Task<AdminClientDetailDto?> UpsertSubscriptionAsync(
        Guid workshopId,
        UpsertSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var workshopExists = await db.Workshops
            .AnyAsync(w => w.Id == workshopId, cancellationToken);
        if (!workshopExists)
            return null;

        var plan = await ResolvePlanAsync(request.PlanCode, cancellationToken);
        if (plan is null)
            return null;

        var seatUsed = await userManager.Users
            .CountAsync(u => u.WorkshopId == workshopId, cancellationToken);
        if (plan.MaxUsers is int max && seatUsed > max)
            return null;

        var now = DateTime.UtcNow;
        DateTime ends;
        if (request.EndsAtUtc.HasValue)
            ends = request.EndsAtUtc.Value.ToUniversalTime();
        else
        {
            var days = request.DurationDays is > 0
                ? request.DurationDays.Value
                : plan.DurationDays;
            ends = now.AddDays(days);
        }

        var current = await db.WorkshopSubscriptions
            .Where(s => s.WorkshopId == workshopId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.EndsAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is not null)
            current.Status = SubscriptionStatus.Suspended;

        var paymentStatus = request.PaymentStatus ?? PaymentStatus.Pending;
        db.WorkshopSubscriptions.Add(new WorkshopSubscription
        {
            WorkshopId = workshopId,
            PlanId = plan.Id,
            StartsAtUtc = now,
            EndsAtUtc = ends,
            Status = SubscriptionStatus.Active,
            PaymentStatus = paymentStatus,
            PaymentAmount = request.PaymentAmount ?? plan.PriceSar,
            PaymentCurrency = string.IsNullOrWhiteSpace(request.PaymentCurrency)
                ? "SAR"
                : request.PaymentCurrency.Trim().ToUpperInvariant(),
            PaymentNote = request.PaymentNote,
            PaymentConfirmedAtUtc = paymentStatus is PaymentStatus.Confirmed or PaymentStatus.Waived
                ? now
                : null
        });

        await db.SaveChangesAsync(cancellationToken);
        return await GetClientAsync(workshopId, cancellationToken);
    }

    public async Task<AdminClientDetailDto?> SuspendSubscriptionAsync(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var actives = await db.WorkshopSubscriptions
            .Where(s => s.WorkshopId == workshopId && s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

        if (actives.Count == 0)
            return await GetClientAsync(workshopId, cancellationToken);

        foreach (var sub in actives)
            sub.Status = SubscriptionStatus.Suspended;

        await db.SaveChangesAsync(cancellationToken);
        return await GetClientAsync(workshopId, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminWorkshopUserDto>> ListUsersAsync(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .Where(u => u.WorkshopId == workshopId)
            .OrderByDescending(u => u.IsPrimaryContact)
            .ThenBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var result = new List<AdminWorkshopUserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var locked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;
            result.Add(new AdminWorkshopUserDto(
                user.Id,
                user.Email!,
                user.DisplayName,
                user.IsPrimaryContact,
                roles.ToList(),
                locked));
        }

        return result;
    }

    public async Task<AddWorkshopUserResult?> AddUserAsync(
        Guid workshopId,
        AddWorkshopUserRequest request,
        bool arabic,
        CancellationToken cancellationToken)
    {
        var workshopExists = await db.Workshops
            .AnyAsync(w => w.Id == workshopId, cancellationToken);
        if (!workshopExists)
            return null;

        var email = request.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            throw new AdminSeatException("Email already registered.");

        var sub = await db.WorkshopSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.WorkshopId == workshopId)
            .OrderByDescending(s => s.EndsAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var seatUsed = await userManager.Users
            .CountAsync(u => u.WorkshopId == workshopId, cancellationToken);
        var max = sub?.Plan.MaxUsers;
        if (max is int limit && seatUsed >= limit)
            throw new AdminSeatException("Seat limit reached for this plan.");

        var tempPassword = string.IsNullOrWhiteSpace(request.Password)
            ? AuthEmailTemplates.GenerateTemporaryPassword()
            : request.Password;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? null
                : request.DisplayName.Trim(),
            EmailConfirmed = true,
            WorkshopId = workshopId,
            IsPrimaryContact = false
        };

        var created = await userManager.CreateAsync(user, tempPassword);
        if (!created.Succeeded)
            throw new AdminSeatException(string.Join("; ", created.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, AppRoles.WorkshopMember);
        var emailSent = await TrySendWelcomeAsync(user, tempPassword, arabic, cancellationToken);
        var roles = await userManager.GetRolesAsync(user);

        return new AddWorkshopUserResult(
            new AdminWorkshopUserDto(
                user.Id,
                user.Email!,
                user.DisplayName,
                user.IsPrimaryContact,
                roles.ToList(),
                false),
            tempPassword,
            emailSent);
    }

    public async Task<AdminTempPasswordResult?> ResetUserPasswordAsync(
        Guid workshopId,
        string userId,
        string? newPassword,
        bool arabic,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.WorkshopId != workshopId)
            return null;

        var tempPassword = string.IsNullOrWhiteSpace(newPassword)
            ? AuthEmailTemplates.GenerateTemporaryPassword()
            : newPassword;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, tempPassword);
        if (!result.Succeeded)
            return null;

        var emailSent = await TrySendWelcomeAsync(user, tempPassword, arabic, cancellationToken);
        return new AdminTempPasswordResult(tempPassword, emailSent);
    }

    public async Task<bool> SetUserLockoutAsync(
        Guid workshopId,
        string userId,
        bool locked,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.WorkshopId != workshopId)
            return false;

        if (locked)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        else
        {
            await userManager.SetLockoutEndDateAsync(user, null);
        }

        return true;
    }

    public async Task<IReadOnlyList<AdminPlanDto>> ListPlansAsync(
        CancellationToken cancellationToken)
    {
        return await db.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.PriceSar)
            .Select(p => new AdminPlanDto(
                p.Id,
                p.Code,
                p.Name,
                p.PriceSar,
                p.DurationDays,
                p.MaxUsers,
                p.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminPlanDto?> UpdatePlanAsync(
        Guid planId,
        UpdateSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var plan = await db.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
        if (plan is null)
            return null;

        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return null;

        if (request.PriceSar < 0)
            return null;

        plan.Name = name;
        plan.PriceSar = request.PriceSar;
        plan.MaxUsers = request.MaxUsers is > 0 ? request.MaxUsers : null;
        if (request.DurationDays is > 0)
            plan.DurationDays = request.DurationDays.Value;
        if (request.IsActive.HasValue)
            plan.IsActive = request.IsActive.Value;

        await db.SaveChangesAsync(cancellationToken);

        return new AdminPlanDto(
            plan.Id,
            plan.Code,
            plan.Name,
            plan.PriceSar,
            plan.DurationDays,
            plan.MaxUsers,
            plan.IsActive);
    }

    public async Task<IReadOnlyList<PublicPlanDto>> ListPublicPlansAsync(
        CancellationToken cancellationToken)
    {
        return await db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceSar)
            .Select(p => new PublicPlanDto(
                p.Code,
                p.Name,
                p.PriceSar,
                p.DurationDays,
                p.MaxUsers))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminReportSummaryDto> GetReportSummaryAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var totalClients = await db.Workshops.CountAsync(cancellationToken);

        var latestSubs = await db.WorkshopSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .ToListAsync(cancellationToken);

        var currentByWorkshop = latestSubs
            .GroupBy(s => s.WorkshopId)
            .Select(g => g.OrderByDescending(x => x.EndsAtUtc).First())
            .ToList();

        int active = 0, expired = 0, suspended = 0, expiring = 0;
        foreach (var sub in currentByWorkshop)
        {
            var status = sub.Status;
            if (status == SubscriptionStatus.Active && sub.EndsAtUtc < now)
                status = SubscriptionStatus.Expired;

            switch (status)
            {
                case SubscriptionStatus.Active:
                    active++;
                    if (sub.EndsAtUtc <= now.AddDays(7))
                        expiring++;
                    break;
                case SubscriptionStatus.Expired:
                    expired++;
                    break;
                case SubscriptionStatus.Suspended:
                    suspended++;
                    break;
            }
        }

        var byPlan = currentByWorkshop
            .Where(s => s.Status == SubscriptionStatus.Active && s.EndsAtUtc >= now)
            .GroupBy(s => new { s.Plan.Code, s.Plan.Name })
            .Select(g => new AdminPlanCountDto(g.Key.Code, g.Key.Name, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        return new AdminReportSummaryDto(
            totalClients,
            active,
            expired,
            suspended,
            expiring,
            byPlan);
    }

    private async Task<bool> TrySendWelcomeAsync(
        ApplicationUser user,
        string temporaryPassword,
        bool arabic,
        CancellationToken cancellationToken)
    {
        try
        {
            await emailSender.SendAsync(
                new EmailMessage(
                    user.Email!,
                    user.DisplayName,
                    AuthEmailTemplates.WelcomeSubject(arabic),
                    AuthEmailTemplates.WelcomeHtml(
                        user.DisplayName ?? string.Empty,
                        user.Email!,
                        temporaryPassword,
                        arabic)),
                cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Welcome email failed for {Email}", user.Email);
            return false;
        }
    }

    private async Task<SubscriptionPlan?> ResolvePlanAsync(
        string planCode,
        CancellationToken cancellationToken)
    {
        var code = planCode.Trim();
        return await db.SubscriptionPlans
            .FirstOrDefaultAsync(
                p => p.Code == code && p.IsActive,
                cancellationToken);
    }
}

public sealed class AdminSeatException : Exception
{
    public AdminSeatException(string message) : base(message)
    {
    }
}
