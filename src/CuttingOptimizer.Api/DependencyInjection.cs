using System.Globalization;
using System.Text.Json.Serialization;
using CuttingOptimizer.Api.Cors;
using CuttingOptimizer.Api.Exceptions;
using CuttingOptimizer.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace CuttingOptimizer.Api;

public static class DependencyInjection
{
    private static readonly CultureInfo[] SupportedCultures =
    [
        new("en"),
        new("ar")
    ];

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MonsterASP / reverse proxies terminate TLS; honor X-Forwarded-* for HTTPS redirects.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddOpenApi();

        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("en");
            options.SupportedCultures = SupportedCultures;
            options.SupportedUICultures = SupportedCultures;
            options.RequestCultureProviders =
            [
                new QueryStringRequestCultureProvider { QueryStringKey = "culture", UIQueryStringKey = "culture" },
                new AcceptLanguageHeaderRequestCultureProvider()
            ];
        });

        var corsOrigins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        services.AddCors(options =>
            options.AddPolicy(CorsOriginPolicy.PolicyName, policy =>
                policy.SetIsOriginAllowed(origin => CorsOriginPolicy.IsAllowed(origin, corsOrigins))
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        var localizationOptions = app.Services
            .GetRequiredService<IOptions<RequestLocalizationOptions>>()
            .Value;
        app.UseRequestLocalization(localizationOptions);

        app.UseExceptionHandler();
        app.UseForwardedHeaders();

        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseCors(CorsOriginPolicy.PolicyName);
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    /// <summary>
    /// Runs migrations and seed data. Failures are logged, not thrown, so the API still listens.
    /// </summary>
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        try
        {
            await IdentityDataSeeder.SeedAllAsync(app.Services);
        }
        catch (Exception ex)
        {
            app.Services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Startup")
                .LogCritical(
                    ex,
                    "Startup seed failed — API will still listen. Fix DB/connection string and restart.");
        }
    }
}
