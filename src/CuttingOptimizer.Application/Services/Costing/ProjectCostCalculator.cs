using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Services.Costing;

/// <summary>
/// Pure (no DB) product BOM explosion and cost aggregation.
/// </summary>
/// <remarks>
/// <para>
/// <b>Aluminum length:</b> when <c>UseSashWidthDivision</c> is true,
/// <c>effectiveW = (W / SashCount) + OverlapMm</c> where <c>SashCount</c>
/// defaults to 1 and <c>OverlapMm</c> to 0 when null; otherwise
/// <c>effectiveW = W</c>. Then
/// <c>LengthMm = round(WidthCoef × effectiveW + HeightCoef × H + OffsetMm)</c>.
/// Lengths ≤ 0 are rejected.
/// </para>
/// <para>
/// <b>Glass:</b> pane width/height use the same sash-width rule on the
/// template coefficients; area is exact
/// <c>AreaM2 = WidthMm × HeightMm / 1_000_000</c> (decimal, no rounding).
/// Line cost = area × price/m² × pane quantity.
/// </para>
/// <para>
/// <b>Accessories:</b> PerProduct = slot Qty × item Qty;
/// PerSash = slot Qty × SashCount × item Qty (SashCount defaults to 1).
/// </para>
/// <para>
/// <b>Aluminum cost allocation (shared bars):</b> after packing, each material
/// has a total bar cost and total consumed bar length. Item shares are based on
/// each item's net piece length for that material
/// (<c>Σ LengthMm × Quantity</c>). Allocated cost is
/// <c>TotalCost × (itemPieceLength / Σ all items' piece lengths)</c>.
/// Because <c>TotalCost</c> already includes waste (kerf + leftover on bars),
/// waste is distributed proportionally with piece length — items that consume
/// more of the material absorb more of the waste cost. If total piece length
/// for a material is zero but packing cost is non-zero, the cost is left
/// unallocated (no item share).
/// </para>
/// </remarks>
public static class ProjectCostCalculator
{
    /// <summary>
    /// Explodes project items into aluminum pieces, glass panes, and accessory
    /// lines without packing or aluminum cost allocation.
    /// </summary>
    /// <exception cref="ProjectCostingValidationException">
    /// When any aluminum or glass dimension formula yields a length ≤ 0.
    /// </exception>
    public static BomExplodeResult BomExplode(
        IReadOnlyList<CostingProjectItemInput> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var aluminum = new List<BomAluminumPiece>();
        var glass = new List<BomGlassPane>();
        var accessories = new List<BomAccessoryLine>();

        foreach (var item in items)
        {
            var sashCount = item.SashCount is null or <= 0
                ? 1
                : item.SashCount.Value;
            var overlapMm = item.OverlapMm ?? 0;

            foreach (var part in item.AluminumParts)
            {
                var lengthMm = ComputeAluminumLengthMm(
                    item.WidthMm,
                    item.HeightMm,
                    sashCount,
                    overlapMm,
                    part);

                if (lengthMm <= 0)
                {
                    throw new ProjectCostingValidationException(
                        $"Aluminum part '{part.DisplayName}' for item {item.ProjectItemId} " +
                        $"produced LengthMm={lengthMm}; must be greater than zero.");
                }

                var quantity = checked(part.QtyPerUnit * item.Quantity);
                aluminum.Add(new BomAluminumPiece(
                    item.ProjectItemId,
                    part.TemplatePartId,
                    part.Role,
                    part.MaterialId,
                    part.DisplayName,
                    lengthMm,
                    quantity));
            }

            foreach (var slot in item.GlassSlots)
            {
                var (widthMm, heightMm) = ComputeGlassSizeMm(
                    item.WidthMm,
                    item.HeightMm,
                    sashCount,
                    overlapMm,
                    slot);

                if (widthMm <= 0 || heightMm <= 0)
                {
                    throw new ProjectCostingValidationException(
                        $"Glass slot '{slot.Name}' for item {item.ProjectItemId} " +
                        $"produced size {widthMm}×{heightMm} mm; both must be greater than zero.");
                }

                var areaM2 = ComputeGlassAreaM2(widthMm, heightMm);
                var paneQuantity = checked(slot.PaneCount * item.Quantity);
                var lineCost = areaM2 * slot.PricePerSqm * paneQuantity;

                glass.Add(new BomGlassPane(
                    item.ProjectItemId,
                    slot.TemplateGlassSlotId,
                    slot.GlassSpecId,
                    slot.Name,
                    widthMm,
                    heightMm,
                    areaM2,
                    paneQuantity,
                    slot.PricePerSqm,
                    lineCost));
            }

            foreach (var slot in item.AccessorySlots)
            {
                var quantity = slot.QuantityRule switch
                {
                    AccessoryQuantityRule.PerSash =>
                        checked(slot.Qty * sashCount * item.Quantity),
                    _ =>
                        checked(slot.Qty * item.Quantity)
                };

                accessories.Add(new BomAccessoryLine(
                    item.ProjectItemId,
                    slot.TemplateAccessorySlotId,
                    slot.AccessoryId,
                    slot.Name,
                    quantity,
                    slot.UnitPrice,
                    slot.UnitPrice * quantity));
            }
        }

        return new BomExplodeResult(aluminum, glass, accessories);
    }

    /// <summary>
    /// Explodes the BOM, allocates packing aluminum costs by piece-length share
    /// (including proportional waste), and aggregates per-item and project totals.
    /// </summary>
    public static ProjectCostResult Calculate(
        IReadOnlyList<CostingProjectItemInput> items,
        IReadOnlyList<MaterialPackingCost> packingCosts)
    {
        ArgumentNullException.ThrowIfNull(packingCosts);

        var bom = BomExplode(items);
        var allocations = AllocateAluminumCosts(bom.AluminumPieces, packingCosts);

        var aluminumByItem = allocations
            .GroupBy(a => a.ProjectItemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.AllocatedCost));

        var glassByItem = bom.GlassPanes
            .GroupBy(g => g.ProjectItemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.LineCost));

        var accessoryByItem = bom.Accessories
            .GroupBy(a => a.ProjectItemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.LineCost));

        var itemIds = items.Select(i => i.ProjectItemId).Distinct().ToList();
        var summaries = new List<ProjectItemCostSummary>(itemIds.Count);

        foreach (var itemId in itemIds)
        {
            aluminumByItem.TryGetValue(itemId, out var alum);
            glassByItem.TryGetValue(itemId, out var glassCost);
            accessoryByItem.TryGetValue(itemId, out var accCost);
            summaries.Add(new ProjectItemCostSummary(
                itemId,
                alum,
                glassCost,
                accCost,
                alum + glassCost + accCost));
        }

        var totalAluminum = summaries.Sum(s => s.AluminumCost);
        var totalGlass = summaries.Sum(s => s.GlassCost);
        var totalAccessory = summaries.Sum(s => s.AccessoryCost);

        return new ProjectCostResult(
            bom,
            allocations,
            summaries,
            totalAluminum,
            totalGlass,
            totalAccessory,
            totalAluminum + totalGlass + totalAccessory);
    }

    /// <summary>
    /// Allocates each material's packed bar cost across project items by their
    /// share of net piece length. Waste embedded in <paramref name="packingCosts"/>
    /// is therefore proportional to piece consumption.
    /// </summary>
    public static IReadOnlyList<ItemAluminumCostAllocation> AllocateAluminumCosts(
        IReadOnlyList<BomAluminumPiece> pieces,
        IReadOnlyList<MaterialPackingCost> packingCosts)
    {
        ArgumentNullException.ThrowIfNull(pieces);
        ArgumentNullException.ThrowIfNull(packingCosts);

        var pieceLengthByItemMaterial = pieces
            .GroupBy(p => (p.ProjectItemId, p.MaterialId))
            .ToDictionary(
                g => g.Key,
                g => g.Sum(p => checked(p.LengthMm * p.Quantity)));

        var allocations = new List<ItemAluminumCostAllocation>();

        foreach (var packing in packingCosts)
        {
            var itemShares = pieceLengthByItemMaterial
                .Where(kv => kv.Key.MaterialId == packing.MaterialId)
                .ToList();

            var totalPieceLength = itemShares.Sum(kv => kv.Value);
            if (totalPieceLength <= 0)
                continue;

            foreach (var (key, pieceLength) in itemShares)
            {
                var share = (decimal)pieceLength / totalPieceLength;
                allocations.Add(new ItemAluminumCostAllocation(
                    key.ProjectItemId,
                    packing.MaterialId,
                    pieceLength,
                    packing.TotalCost * share));
            }
        }

        return allocations;
    }

    public static int ComputeAluminumLengthMm(
        int widthMm,
        int heightMm,
        int sashCount,
        int overlapMm,
        CostingAluminumPartInput part)
    {
        var effectiveW = ResolveEffectiveWidth(
            widthMm,
            sashCount,
            overlapMm,
            part.UseSashWidthDivision);

        var raw = (part.WidthCoef * effectiveW)
                  + (part.HeightCoef * heightMm)
                  + part.OffsetMm;

        return (int)Math.Round(raw, MidpointRounding.AwayFromZero);
    }

    public static (int WidthMm, int HeightMm) ComputeGlassSizeMm(
        int widthMm,
        int heightMm,
        int sashCount,
        int overlapMm,
        CostingGlassSlotInput slot)
    {
        var effectiveW = ResolveEffectiveWidth(
            widthMm,
            sashCount,
            overlapMm,
            slot.UseSashWidthDivision);

        var glassW = (slot.WidthCoefW * effectiveW)
                     + (slot.HeightCoefW * heightMm)
                     + slot.OffsetWMm;
        var glassH = (slot.WidthCoefH * effectiveW)
                     + (slot.HeightCoefH * heightMm)
                     + slot.OffsetHMm;

        return (
            (int)Math.Round(glassW, MidpointRounding.AwayFromZero),
            (int)Math.Round(glassH, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Exact m² from integer millimetre dimensions (no area rounding).
    /// </summary>
    public static decimal ComputeGlassAreaM2(int widthMm, int heightMm) =>
        (decimal)widthMm * heightMm / 1_000_000m;

    private static decimal ResolveEffectiveWidth(
        int widthMm,
        int sashCount,
        int overlapMm,
        bool useSashWidthDivision)
    {
        if (!useSashWidthDivision)
            return widthMm;

        var count = sashCount <= 0 ? 1 : sashCount;
        return ((decimal)widthMm / count) + overlapMm;
    }
}
