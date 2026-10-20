using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Behaviors;
using CuttingOptimizer.Application.Services.Optimization;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CuttingOptimizer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddSingleton<ICuttingPlanOptimizer, CuttingPlanOptimizer>();
        services.AddSingleton<ICuttingPlanAlternativesGenerator, CuttingPlanAlternativesGenerator>();

        return services;
    }
}
