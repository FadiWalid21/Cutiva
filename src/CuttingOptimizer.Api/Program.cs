using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using CuttingOptimizer.Api.Exceptions;
using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Auth;
using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Behaviors;
using CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;
using CuttingOptimizer.Application.Options;
using CuttingOptimizer.Application.Services.Optimization;
using CuttingOptimizer.Infrastructure.Auth;
using CuttingOptimizer.Infrastructure.Identity;
using CuttingOptimizer.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// MonsterASP / reverse proxies terminate TLS; honor X-Forwarded-* for HTTPS redirects.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

builder.Services.AddLocalization();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders =
    [
        new QueryStringRequestCultureProvider { QueryStringKey = "culture", UIQueryStringKey = "culture" },
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

builder.Services.Configure<FeaturesOptions>(
    builder.Configuration.GetSection(FeaturesOptions.SectionName));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.PostConfigure<JwtOptions>(options =>
{
    if (string.IsNullOrWhiteSpace(options.Key) || options.Key.Length < 32)
        options.Key = "CutivaDevSigningKey_ChangeInProduction_32chars!";
});

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
    jwtOptions.Key = "CutivaDevSigningKey_ChangeInProduction_32chars!";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IApplicationDbContext>(
    provider => provider.GetRequiredService<ApplicationDbContext>());

builder.Services
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

builder.Services
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

builder.Services.AddAuthorization();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.SetIsOriginAllowed(origin => IsAllowedCorsOrigin(origin, corsOrigins))
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.Configure<CuttingOptimizer.Infrastructure.Email.BrevoOptions>(
    builder.Configuration.GetSection(
        CuttingOptimizer.Infrastructure.Email.BrevoOptions.SectionName));

builder.Services.AddHttpClient<
    CuttingOptimizer.Application.Abstractions.Email.IEmailSender,
    CuttingOptimizer.Infrastructure.Email.BrevoEmailSender>();

builder.Services.AddSingleton<ICuttingPlanOptimizer, CuttingPlanOptimizer>();
builder.Services.AddSingleton<ICuttingPlanAlternativesGenerator, CuttingPlanAlternativesGenerator>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<IAuthService, IdentityAuthService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<
    CuttingOptimizer.Application.Abstractions.Admin.IAdminClientService,
    CuttingOptimizer.Infrastructure.Admin.AdminClientService>();

builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(
        typeof(CreateWorkshopCommand).Assembly));

builder.Services.AddValidatorsFromAssembly(
    typeof(CreateWorkshopCommand).Assembly);

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

var localizationOptions = app.Services
    .GetRequiredService<IOptions<RequestLocalizationOptions>>()
    .Value;
app.UseRequestLocalization(localizationOptions);

app.UseExceptionHandler();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await CuttingOptimizer.Infrastructure.Persistence.IdentityDataSeeder.SeedAllAsync(
    app.Services);

app.Run();

static bool IsAllowedCorsOrigin(string? origin, string[] configured)
{
    if (string.IsNullOrWhiteSpace(origin))
        return false;

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        return false;

    if (configured.Any(o =>
            string.Equals(o, origin, StringComparison.OrdinalIgnoreCase)))
        return true;

    // Local Angular dev
    if (uri.IsLoopback && (uri.Port == 4200 || uri.Port == 4201))
        return true;

    // Vercel production + preview deployments (https://*.vercel.app)
    if (uri.Scheme == Uri.UriSchemeHttps
        && uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
        return true;

    return false;
}
