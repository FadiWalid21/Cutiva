using System.Globalization;
using CuttingOptimizer.Api.Exceptions;
using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Behaviors;
using CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;
using CuttingOptimizer.Application.Services.Optimization;
using CuttingOptimizer.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IApplicationDbContext>(
    provider => provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddSingleton<ICuttingPlanOptimizer, CuttingPlanOptimizer>();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
