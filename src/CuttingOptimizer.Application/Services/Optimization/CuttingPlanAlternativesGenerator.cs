using CuttingOptimizer.Application.Abstractions.Optimization;

namespace CuttingOptimizer.Application.Services.Optimization;

public sealed class CuttingPlanAlternativesGenerator(
    ICuttingPlanOptimizer optimizer)
    : ICuttingPlanAlternativesGenerator
{
    public IReadOnlyList<CuttingPlanAlternativeCandidate> Generate(
        int kerfMm,
        IReadOnlyList<OptimizationRequiredCut> requiredCuts,
        IReadOnlyList<OptimizationStockOption> stockOptions,
        bool useRemnants = true)
    {
        var pricedOrRemnant = stockOptions
            .Where(o => o.IsRemnant || o.UnitPrice > 0)
            .ToList();

        var effectiveStock = useRemnants
            ? pricedOrRemnant
            : pricedOrRemnant.Where(o => !o.IsRemnant).ToList();

        var candidates = new List<CuttingPlanAlternativeCandidate>();
        var catalog = effectiveStock.Where(o => !o.IsRemnant).ToList();

        if (effectiveStock.Count == 0)
        {
            var empty = optimizer.Optimize(new CuttingOptimizationInput(
                kerfMm,
                requiredCuts,
                effectiveStock,
                UseRemnants: useRemnants));
            return
            [
                new CuttingPlanAlternativeCandidate(
                    "MixedCheapestUnit",
                    empty,
                    BuildBarsSummary(empty))
            ];
        }

        candidates.Add(Run(
            "MixedCheapestUnit",
            kerfMm,
            requiredCuts,
            effectiveStock,
            BarOpenPreference.CheapestUnitPrice,
            allowedCatalog: null,
            useRemnants));

        candidates.Add(Run(
            "MixedLeastWasteOpen",
            kerfMm,
            requiredCuts,
            effectiveStock,
            BarOpenPreference.LeastLeftover,
            allowedCatalog: null,
            useRemnants));

        foreach (var group in catalog.GroupBy(option => option.LengthMm).OrderBy(g => g.Key))
        {
            var representative = group
                .OrderBy(option => option.UnitPrice)
                .ThenBy(option => option.StockBarId)
                .First();

            var allowed = new HashSet<Guid> { representative.StockBarId };
            var code = $"SingleSku_{representative.LengthMm}";

            candidates.Add(Run(
                code,
                kerfMm,
                requiredCuts,
                effectiveStock,
                BarOpenPreference.CheapestUnitPrice,
                allowed,
                useRemnants));
        }

        return Deduplicate(candidates);
    }

    private CuttingPlanAlternativeCandidate Run(
        string strategyCode,
        int kerfMm,
        IReadOnlyList<OptimizationRequiredCut> requiredCuts,
        IReadOnlyList<OptimizationStockOption> stockOptions,
        BarOpenPreference preference,
        IReadOnlySet<Guid>? allowedCatalog,
        bool useRemnants)
    {
        var result = optimizer.Optimize(new CuttingOptimizationInput(
            kerfMm,
            requiredCuts,
            stockOptions,
            preference,
            allowedCatalog,
            useRemnants));

        return new CuttingPlanAlternativeCandidate(
            strategyCode,
            result,
            BuildBarsSummary(result));
    }

    private static IReadOnlyList<CuttingPlanAlternativeCandidate> Deduplicate(
        IReadOnlyList<CuttingPlanAlternativeCandidate> candidates)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var unique = new List<CuttingPlanAlternativeCandidate>();

        foreach (var candidate in candidates)
        {
            var key =
                $"{candidate.Result.TotalMaterialCost:F2}|"
                + $"{candidate.Result.TotalBarsUsed}|"
                + $"{candidate.Result.IsComplete}|"
                + $"{candidate.BarsSummary}";

            if (!seen.Add(key))
                continue;

            unique.Add(candidate);
        }

        return unique;
    }

    internal static string BuildBarsSummary(CuttingOptimizationResult result)
    {
        if (result.Bars.Count == 0)
            return "—";

        return string.Join(
            " · ",
            result.Bars
                .GroupBy(bar => bar.LengthMm)
                .OrderByDescending(group => group.Key)
                .Select(group =>
                {
                    var cm = group.Key / 10.0;
                    var label = NumberIsInteger(cm) ? $"{(int)cm}" : cm.ToString("0.#");
                    return $"{group.Count()}×{label}";
                }));
    }

    private static bool NumberIsInteger(double value) =>
        Math.Abs(value - Math.Round(value)) < 0.0001;
}
