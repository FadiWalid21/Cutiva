using CuttingOptimizer.Api;
using CuttingOptimizer.Application;
using CuttingOptimizer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseApiPipeline();
await app.SeedDatabaseAsync();

app.Run();
