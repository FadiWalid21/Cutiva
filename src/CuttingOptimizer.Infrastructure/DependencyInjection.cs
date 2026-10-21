using System.Security.Claims;
using System.Text;
using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Admin;
using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Application.Abstractions.Email;
using CuttingOptimizer.Application.Abstractions.Entitlements;
using CuttingOptimizer.Application.Abstractions.Pricing;
using CuttingOptimizer.Application.Options;
using CuttingOptimizer.Infrastructure.Admin;
using CuttingOptimizer.Infrastructure.Auth;
using CuttingOptimizer.Infrastructure.Email;
using CuttingOptimizer.Infrastructure.Entitlements;
using CuttingOptimizer.Infrastructure.Identity;
using CuttingOptimizer.Infrastructure.Persistence;
using CuttingOptimizer.Infrastructure.Pricing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace CuttingOptimizer.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Used when Jwt:Key is missing or shorter than 32 characters.</summary>
    private const string FallbackJwtKey = "CutivaDevSigningKey_ChangeInProduction_32chars!";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<FeaturesOptions>(
            configuration.GetSection(FeaturesOptions.SectionName));

        services
            .AddPersistence(configuration)
            .AddIdentityAndAuth(configuration, environment.IsDevelopment())
            .AddExternalServices(configuration);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddIdentityAndAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.PostConfigure<JwtOptions>(options =>
            ApplyFallbackJwtKey(options, isDevelopment));

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                         ?? new JwtOptions();
        ApplyFallbackJwtKey(jwtOptions, isDevelopment);

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization();

        services.AddSingleton<JwtTokenService>();
        services.AddScoped<IAuthService, IdentityAuthService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();

        return services;
    }

    private static IServiceCollection AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BrevoOptions>(configuration.GetSection(BrevoOptions.SectionName));
        services.AddHttpClient<IEmailSender, BrevoEmailSender>();

        services.AddMemoryCache();
        services.AddHttpClient("GeoIp", client =>
        {
            client.BaseAddress = new Uri("http://ip-api.com/");
            client.Timeout = TimeSpan.FromSeconds(3);
        });
        services.AddHttpClient("FxRates", client =>
        {
            client.BaseAddress = new Uri("https://open.er-api.com/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddScoped<IPricingDisplayService, PricingDisplayService>();
        services.AddScoped<IPlanEntitlementService, PlanEntitlementService>();
        services.AddScoped<IAdminClientService, AdminClientService>();

        return services;
    }

    private static void ApplyFallbackJwtKey(JwtOptions options, bool isDevelopment)
    {
        if (!string.IsNullOrWhiteSpace(options.Key) && options.Key.Length >= 32)
            return;

        if (!isDevelopment)
        {
            throw new InvalidOperationException(
                "Jwt:Key must be set to at least 32 characters.");
        }

        options.Key = FallbackJwtKey;
    }
}
