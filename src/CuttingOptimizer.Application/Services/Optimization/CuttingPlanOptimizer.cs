using CuttingOptimizer.Application.Abstractions.Optimization;

namespace CuttingOptimizer.Application.Services.Optimization;

/// <summary>
/// Cost-aware First-Fit Decreasing (FFD) cutting planner.
/// Prefers workshop remnant bars (limited qty) before market catalog (unlimited).
/// Each piece is placed on a single bar (no splicing).
/// </summary>
public sealed class CuttingPlanOptimizer : ICuttingPlanOptimizer
{
    private const string ContinuityNote =
        "Each piece is cut from a single bar; splicing pieces is not allowed. ";

    public CuttingOptimizationResult Optimize(CuttingOptimizationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.KerfMm < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input),
                input.KerfMm,
                "KerfMm must be greater than or equal to zero.");
        }

        var stockOptions = input.StockOptions
            .Where(option =>
            {
                if (option.IsRemnant)
                    return input.UseRemnants;

                return input.AllowedCatalogStockBarIds is null
                    || input.AllowedCatalogStockBarIds.Contains(option.StockBarId);
            })
            .ToList();

        var pieces = ExpandAndSortPieces(input.RequiredCuts);
        var stockById = stockOptions.ToDictionary(option => option.StockBarId);
        var remainingQty = stockOptions.ToDictionary(
            option => option.StockBarId,
            option => option.IsRemnant
                ? option.AvailableQuantity
                : int.MaxValue / 4);

        var openBars = new List<OpenBar>();
        var unallocated = new List<UnallocatedPiece>();

        foreach (var piece in pieces)
        {
            if (TryPlaceOnOpenBar(openBars, piece, input.KerfMm))
                continue;

            if (TryOpenNewBar(
                    openBars,
                    stockById,
                    remainingQty,
                    piece,
                    input.BarOpenPreference))
                continue;

            unallocated.Add(new UnallocatedPiece(
                piece.RequiredCutId,
                piece.Name,
                piece.LengthMm));
        }

        var bars = openBars
            .Select(bar =>
            {
                var kerfMm = Math.Max(bar.Pieces.Count - 1, 0) * input.KerfMm;
                var usedLength = bar.Pieces.Sum(p => p.LengthMm) + kerfMm;
                var remaining = bar.LengthMm - usedLength;

                return new OptimizedBarAllocation(
                    bar.StockBarId,
                    bar.LengthMm,
                    bar.UnitPrice,
                    kerfMm,
                    remaining,
                    bar.Pieces.ToList());
            })
            .ToList();

        var totalKerfMm = bars.Sum(bar => bar.KerfMm);
        var totalLeftoverMm = bars.Sum(bar => bar.RemainingLengthMm);
        // Remnants are priced 0 — only market catalog bars contribute to buy cost.
        var totalMaterialCost = bars.Sum(bar => bar.UnitPrice);

        var preferenceNote = input.BarOpenPreference == BarOpenPreference.LeastLeftover
            ? "Bar open preference: least leftover, then unit price. "
            : "Bar open preference: cheapest unit price, then leftover. ";

        var remnantNote = input.UseRemnants
            ? "Remnant inventory is preferred before buying market bars. "
            : "Market purchase only — remnant warehouse stock was not used. ";

        var strategyNote =
            ContinuityNote
            + remnantNote
            + preferenceNote
            + "Cost-aware FFD heuristic; not globally optimal. "
            + "Market catalog lengths are unlimited supply.";

        return new CuttingOptimizationResult(
            IsComplete: unallocated.Count == 0,
            TotalBarsUsed: bars.Count,
            TotalMaterialCost: totalMaterialCost,
            TotalKerfMm: totalKerfMm,
            TotalLeftoverMm: totalLeftoverMm,
            Bars: bars,
            UnallocatedPieces: unallocated,
            StrategyNote: strategyNote);
    }

    private static List<PieceCandidate> ExpandAndSortPieces(
        IReadOnlyList<OptimizationRequiredCut> requiredCuts)
    {
        var pieces = new List<PieceCandidate>();

        foreach (var cut in requiredCuts)
        {
            if (cut.Quantity <= 0 || cut.LengthMm <= 0)
                continue;

            for (var i = 0; i < cut.Quantity; i++)
            {
                pieces.Add(new PieceCandidate(
                    cut.RequiredCutId,
                    cut.Name,
                    cut.LengthMm));
            }
        }

        return pieces
            .OrderByDescending(piece => piece.LengthMm)
            .ThenBy(piece => piece.RequiredCutId)
            .ToList();
    }

    private static bool TryPlaceOnOpenBar(
        List<OpenBar> openBars,
        PieceCandidate piece,
        int kerfMm)
    {
        foreach (var bar in openBars)
        {
            var extraKerf = bar.Pieces.Count > 0 ? kerfMm : 0;
            if (bar.RemainingLengthMm < piece.LengthMm + extraKerf)
                continue;

            bar.Pieces.Add(new AllocatedPiece(
                piece.RequiredCutId,
                piece.Name,
                piece.LengthMm));
            bar.RemainingLengthMm -= piece.LengthMm + extraKerf;
            return true;
        }

        return false;
    }

    private static bool TryOpenNewBar(
        List<OpenBar> openBars,
        Dictionary<Guid, OptimizationStockOption> stockById,
        Dictionary<Guid, int> remainingQty,
        PieceCandidate piece,
        BarOpenPreference preference)
    {
        var remnantPick = PickBestOption(
            stockById.Values.Where(o =>
                o.IsRemnant
                && remainingQty[o.StockBarId] > 0
                && o.LengthMm >= piece.LengthMm),
            piece,
            preference);

        var best = remnantPick
            ?? PickBestOption(
                stockById.Values.Where(o =>
                    !o.IsRemnant
                    && remainingQty[o.StockBarId] > 0
                    && o.LengthMm >= piece.LengthMm),
                piece,
                preference);

        if (best is null)
            return false;

        remainingQty[best.StockBarId]--;

        var openBar = new OpenBar(
            best.StockBarId,
            best.LengthMm,
            best.UnitPrice,
            best.LengthMm);

        openBar.Pieces.Add(new AllocatedPiece(
            piece.RequiredCutId,
            piece.Name,
            piece.LengthMm));
        openBar.RemainingLengthMm -= piece.LengthMm;

        openBars.Add(openBar);
        return true;
    }

    private static OptimizationStockOption? PickBestOption(
        IEnumerable<OptimizationStockOption> options,
        PieceCandidate piece,
        BarOpenPreference preference)
    {
        OptimizationStockOption? best = null;
        var bestLeftover = int.MaxValue;
        var bestPrice = decimal.MaxValue;

        foreach (var option in options)
        {
            var leftover = option.LengthMm - piece.LengthMm;
            var isBetter = preference switch
            {
                BarOpenPreference.LeastLeftover =>
                    best is null
                    || leftover < bestLeftover
                    || (leftover == bestLeftover && option.UnitPrice < bestPrice)
                    || (leftover == bestLeftover
                        && option.UnitPrice == bestPrice
                        && option.StockBarId.CompareTo(best!.StockBarId) < 0),
                _ =>
                    best is null
                    || option.UnitPrice < bestPrice
                    || (option.UnitPrice == bestPrice && leftover < bestLeftover)
                    || (option.UnitPrice == bestPrice
                        && leftover == bestLeftover
                        && option.StockBarId.CompareTo(best!.StockBarId) < 0)
            };

            if (!isBetter)
                continue;

            best = option;
            bestLeftover = leftover;
            bestPrice = option.UnitPrice;
        }

        return best;
    }

    private sealed class OpenBar(
        Guid stockBarId,
        int lengthMm,
        decimal unitPrice,
        int remainingLengthMm)
    {
        public Guid StockBarId { get; } = stockBarId;
        public int LengthMm { get; } = lengthMm;
        public decimal UnitPrice { get; } = unitPrice;
        public int RemainingLengthMm { get; set; } = remainingLengthMm;
        public List<AllocatedPiece> Pieces { get; } = [];
    }

    private sealed record PieceCandidate(
        Guid RequiredCutId,
        string Name,
        int LengthMm);
}
