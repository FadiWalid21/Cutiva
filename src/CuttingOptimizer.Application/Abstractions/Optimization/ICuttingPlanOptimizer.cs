using CuttingOptimizer.Application.Services.Optimization;

namespace CuttingOptimizer.Application.Abstractions.Optimization;

public interface ICuttingPlanOptimizer
{
    CuttingOptimizationResult Optimize(CuttingOptimizationInput input);
}
