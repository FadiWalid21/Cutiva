using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Services.Optimization;

namespace CuttingOptimizer.Application.Tests;

public sealed class CuttingPlanUseRemnantsTests
{
    private static readonly Guid CatalogId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid RemnantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid CutId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void Alternatives_WithRemnants_CheaperThanMarketOnly()
    {
        var cuts = new List<OptimizationRequiredCut>
        {
            new(CutId, "rail", LengthMm: 800, Quantity: 1)
        };

        var stock = new List<OptimizationStockOption>
        {
            new(CatalogId, LengthMm: 6000, UnitPrice: 500m, AvailableQuantity: int.MaxValue / 4, IsRemnant: false),
            new(RemnantId, LengthMm: 1200, UnitPrice: 0m, AvailableQuantity: 1, IsRemnant: true)
        };

        var generator = new CuttingPlanAlternativesGenerator(new CuttingPlanOptimizer());

        var withRemnants = generator.Generate(0, cuts, stock, useRemnants: true);
        var marketOnly = generator.Generate(0, cuts, stock, useRemnants: false);

        var cheapestWith = CheapestComplete(withRemnants);
        var cheapestMarket = CheapestComplete(marketOnly);

        Assert.NotNull(cheapestWith);
        Assert.NotNull(cheapestMarket);
        Assert.True(cheapestWith.IsComplete);
        Assert.True(cheapestMarket.IsComplete);
        Assert.Equal(0m, cheapestWith.TotalMaterialCost);
        Assert.Equal(500m, cheapestMarket.TotalMaterialCost);
        Assert.True(cheapestMarket.TotalMaterialCost > cheapestWith.TotalMaterialCost);
    }

    [Fact]
    public void Optimizer_UseRemnantsFalse_IgnoresRemnantOptionsInInput()
    {
        var optimizer = new CuttingPlanOptimizer();
        var cuts = new List<OptimizationRequiredCut>
        {
            new(CutId, "rail", LengthMm: 800, Quantity: 1)
        };
        var stock = new List<OptimizationStockOption>
        {
            new(CatalogId, LengthMm: 6000, UnitPrice: 500m, AvailableQuantity: int.MaxValue / 4, IsRemnant: false),
            new(RemnantId, LengthMm: 1200, UnitPrice: 0m, AvailableQuantity: 1, IsRemnant: true)
        };

        var result = optimizer.Optimize(new CuttingOptimizationInput(
            KerfMm: 0,
            RequiredCuts: cuts,
            StockOptions: stock,
            UseRemnants: false));

        Assert.True(result.IsComplete);
        Assert.Equal(500m, result.TotalMaterialCost);
        Assert.DoesNotContain(result.Bars, b => b.StockBarId == RemnantId);
        Assert.Contains("Market purchase only", result.StrategyNote);
    }

    private static CuttingOptimizationResult? CheapestComplete(
        IReadOnlyList<CuttingPlanAlternativeCandidate> candidates) =>
        candidates
            .Where(c => c.Result.IsComplete)
            .OrderBy(c => c.Result.TotalMaterialCost)
            .Select(c => c.Result)
            .FirstOrDefault();
}
