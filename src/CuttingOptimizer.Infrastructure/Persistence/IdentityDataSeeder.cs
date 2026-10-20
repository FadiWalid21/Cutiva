using CuttingOptimizer.Domain.Constants;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    public const string DefaultAdminEmail = "fadialhallag@gmail.com";
    public const string DefaultAdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentityDataSeeder");
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        foreach (var role in new[]
                 {
                     AppRoles.Admin,
                     AppRoles.WorkshopOwner,
                     AppRoles.WorkshopMember
                 })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await EnsurePlansAsync(db);
        await EnsurePlanFeaturesAsync(db);
        await EnsureFxFallbackAsync(db);

        var admin = await userManager.FindByEmailAsync(DefaultAdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = DefaultAdminEmail,
                Email = DefaultAdminEmail,
                DisplayName = "Platform Admin",
                EmailConfirmed = true,
                WorkshopId = null
            };

            var created = await userManager.CreateAsync(admin, DefaultAdminPassword);
            if (!created.Succeeded)
            {
                logger.LogError(
                    "Failed to create default admin: {Errors}",
                    string.Join("; ", created.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation(
                "Created default admin {Email}",
                DefaultAdminEmail);
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
    }

    /// <summary>
    /// Runs identity seed then product template catalog seed (insert-if-empty).
    /// </summary>
    public static async Task SeedAllAsync(IServiceProvider services)
    {
        await SeedAsync(services);
        await ProductTemplateSeeder.SeedAsync(services);
    }

    private static async Task EnsurePlansAsync(ApplicationDbContext db)
    {
        var specs = new (string Code, string Name, decimal Price, int? MaxUsers)[]
        {
            (SubscriptionPlanCodes.Starter, "Starter", 99m, 1),
            (SubscriptionPlanCodes.Workshop, "Workshop", 149m, 3),
            (SubscriptionPlanCodes.Studio, "Studio", 249m, null)
        };

        foreach (var (code, name, price, maxUsers) in specs)
        {
            var existing = await db.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Code == code);

            // Insert-only: never overwrite admin-edited Name / Price / MaxUsers.
            if (existing is null)
            {
                db.SubscriptionPlans.Add(new SubscriptionPlan
                {
                    Code = code,
                    Name = name,
                    PriceSar = price,
                    DurationDays = 30,
                    MaxUsers = maxUsers,
                    IsActive = true
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsurePlanFeaturesAsync(ApplicationDbContext db)
    {
        foreach (var (code, name, sort) in PlanFeatureCodes.Catalog)
        {
            var existing = await db.FeatureDefinitions
                .FirstOrDefaultAsync(f => f.Code == code);
            if (existing is null)
            {
                db.FeatureDefinitions.Add(new FeatureDefinition
                {
                    Code = code,
                    Name = name,
                    SortOrder = sort
                });
            }
            else
            {
                // Keep Name/Sort in sync with catalog; admin toggles live on plan rows.
                existing.Name = name;
                existing.SortOrder = sort;
            }
        }

        await db.SaveChangesAsync();

        var plans = await db.SubscriptionPlans
            .Include(p => p.Features)
            .ToListAsync();

        foreach (var plan in plans)
        {
            var defaults = DefaultFeaturesForPlan(plan.Code);
            foreach (var (code, _, _) in PlanFeatureCodes.Catalog)
            {
                if (plan.Features.Any(f => f.FeatureCode == code))
                    continue;

                plan.Features.Add(new SubscriptionPlanFeature
                {
                    PlanId = plan.Id,
                    FeatureCode = code,
                    IsEnabled = defaults.Contains(code)
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static HashSet<string> DefaultFeaturesForPlan(string planCode)
    {
        // Starter: catalog only. Workshop+: paid capabilities. Studio: all.
        if (string.Equals(planCode, SubscriptionPlanCodes.Starter, StringComparison.Ordinal))
        {
            return
            [
                PlanFeatureCodes.Resources
            ];
        }

        if (string.Equals(planCode, SubscriptionPlanCodes.Workshop, StringComparison.Ordinal)
            || string.Equals(planCode, SubscriptionPlanCodes.Studio, StringComparison.Ordinal))
        {
            return PlanFeatureCodes.All.ToHashSet(StringComparer.Ordinal);
        }

        return [];
    }

    private static async Task EnsureFxFallbackAsync(ApplicationDbContext db)
    {
        const string from = "SAR";
        const string to = "EGP";
        const decimal seed = 13.0m;

        var exists = await db.FxRateSnapshots
            .AnyAsync(x => x.FromCurrency == from && x.ToCurrency == to);
        if (exists)
            return;

        db.FxRateSnapshots.Add(new FxRateSnapshot
        {
            FromCurrency = from,
            ToCurrency = to,
            Rate = seed,
            FetchedAtUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
