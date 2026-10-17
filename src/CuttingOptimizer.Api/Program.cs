using CuttingOptimizer.Api.Exceptions;
using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Behaviors;
using CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;
using CuttingOptimizer.Application.Services.Optimization;
using CuttingOptimizer.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
