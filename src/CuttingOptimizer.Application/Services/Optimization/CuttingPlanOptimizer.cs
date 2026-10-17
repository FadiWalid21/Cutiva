using CuttingOptimizer.Application.Abstractions.Optimization;

namespace CuttingOptimizer.Application.Services.Optimization;

/// <summary>
/// Cost-aware First-Fit Decreasing (FFD) cutting planner.
/// </summary>
/// <remarks>
/// <para>
/// <b>Kerf convention:</b> lengths are in millimeters. For a bar with
/// <c>n</c> assigned pieces, kerf is charged only between consecutive pieces:
/// <c>totalKerf = max(n - 1, 0) * kerfMm</c> (none at the bar ends).
/// Used length = sum(piece lengths) + totalKerf.
/// Remaining length = stockLengthMm - usedLength.
/// A piece fits an open bar only when remaining length is at least
/// <c>pieceLength + (barAlreadyHasPieces ? kerfMm : 0)</c>.
/// Kerf must be supplied by the caller; this service never assumes a default.
/// </para>
/// <para>
/// <b>Objectives (local greedy choices):</b> when opening a new bar, prefer
/// lower unit price, then lower leftover after placing the piece.
/// This heuristic does <b>not</b> guarantee a globally optimal solution
/// for total cost or leftover.
/// </para>
/// <para>
/// Offcut reuse and inventory mutation are out of scope.
/// </para>
/// </remarks>
public sealed class CuttingPlanOptimizer : ICuttingPlanOptimizer
{
    private const string StrategyNote =
        "Cost-aware First-Fit Decreasing heuristic. "
        + "Does not guarantee a globally optimal plan. "
        + "Kerf is charged between consecutive pieces only (n-1 gaps). "
        + "Inventory is not reserved or consumed.";

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

        var pieces = ExpandAndSortPieces(input.RequiredCuts);
        var remainingQty = input.StockOptions.ToDictionary(
            option => option.StockBarId,
            option => option.AvailableQuantity);
        var stockById = input.StockOptions.ToDictionary(
            option => option.StockBarId);

        var openBars = new List<OpenBar>();
        var unallocated = new List<UnallocatedPiece>();

        foreach (var piece in pieces)
        {
            if (TryPlaceOnOpenBar(openBars, piece, input.KerfMm))
                continue;

            if (TryOpenNewBar(
                    openBars,
                    remainingQty,
                    stockById,
                    piece))
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
        var totalMaterialCost = bars.Sum(bar => bar.UnitPrice);

        return new CuttingOptimizationResult(
            IsComplete: unallocated.Count == 0,
            TotalBarsUsed: bars.Count,
            TotalMaterialCost: totalMaterialCost,
            TotalKerfMm: totalKerfMm,
            TotalLeftoverMm: totalLeftoverMm,
            Bars: bars,
            UnallocatedPieces: unallocated,
            StrategyNote: StrategyNote);
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
        Dictionary<Guid, int> remainingQty,
        Dictionary<Guid, OptimizationStockOption> stockById,
        PieceCandidate piece)
    {
        OptimizationStockOption? best = null;
        var bestLeftover = int.MaxValue;

        foreach (var option in stockById.Values)
        {
            if (remainingQty[option.StockBarId] <= 0)
                continue;

            if (option.LengthMm < piece.LengthMm)
                continue;

            var leftover = option.LengthMm - piece.LengthMm;

            if (best is null
                || option.UnitPrice < best.UnitPrice
                || (option.UnitPrice == best.UnitPrice
                    && leftover < bestLeftover)
                || (option.UnitPrice == best.UnitPrice
                    && leftover == bestLeftover
                    && option.StockBarId.CompareTo(best.StockBarId) < 0))
            {
                best = option;
                bestLeftover = leftover;
            }
        }

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
