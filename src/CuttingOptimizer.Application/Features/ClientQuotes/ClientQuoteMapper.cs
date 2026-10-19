using System.Text.Json;
using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Accessories;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.ClientQuotes;

internal static class ClientQuoteMapper
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<ClientQuoteDto?> LoadDto(
        IApplicationDbContext context,
        Guid quoteId,
        CancellationToken cancellationToken)
    {
        var quote = await context.ClientQuotes
            .AsNoTracking()
            .Where(q => q.Id == quoteId)
            .Select(q => new
            {
                q.Id,
                q.ProjectId,
                ProjectName = q.Project.Name,
                q.Project.CustomerName,
                q.CuttingPlanId,
                WorkshopId = q.Project.WorkshopId,
                WorkshopName = q.Project.Workshop.Name,
                ProductionPricePerM2 = q.Project.Workshop.ProductionPricePerM2,
                MaterialCode = q.CuttingPlan != null
                    ? q.CuttingPlan.Material.Code
                    : string.Empty,
                MaterialName = q.CuttingPlan != null
                    ? q.CuttingPlan.Material.Name
                    : string.Empty,
                MaterialCost = q.CuttingPlan != null
                    ? q.CuttingPlan.TotalMaterialCost
                    : 0m,
                q.Notes,
                q.CreatedAtUtc,
                Surcharges = q.Surcharges
                    .OrderBy(s => s.Reason)
                    .Select(s => new ClientQuoteSurchargeDto(s.Id, s.Reason, s.Amount))
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (quote is null)
            return null;

        var sections = await context.ProjectSections
            .AsNoTracking()
            .Where(s => s.ProjectId == quote.ProjectId)
            .OrderBy(s => s.Name)
            .Select(s => new QuoteSectionPreviewDto(
                s.Id,
                s.Name,
                s.Sector.Code,
                s.Sector.Name,
                s.HeightMm,
                s.WidthMm,
                s.UnitQuantity,
                s.Sector.MaterialId != null))
            .ToListAsync(cancellationToken);

        // Section-linked accessories (legacy manual flow).
        var accessoryRows = await context.ProjectSectionAccessories
            .AsNoTracking()
            .Where(a => a.ProjectSection.ProjectId == quote.ProjectId)
            .Select(a => new
            {
                a.AccessoryId,
                a.Accessory.Name,
                a.Accessory.UnitPrice,
                a.Quantity
            })
            .ToListAsync(cancellationToken);

        var sectionAccessories = accessoryRows
            .GroupBy(a => new { a.AccessoryId, a.Name, a.UnitPrice })
            .Select(g =>
            {
                var qty = g.Sum(x => x.Quantity);
                return new QuoteAccessoryLineDto(
                    g.Key.AccessoryId,
                    g.Key.Name,
                    g.Key.UnitPrice,
                    qty,
                    g.Key.UnitPrice * qty);
            })
            .ToList();

        // Product-item calculation snapshots (Resources V2 costing).
        var snapshots = await context.ProjectItems
            .AsNoTracking()
            .Where(i => i.ProjectId == quote.ProjectId
                        && i.CalculationSnapshotJson != null)
            .Select(i => i.CalculationSnapshotJson!)
            .ToListAsync(cancellationToken);

        var productGlass = new List<QuoteGlassLineDto>();
        var productAccessories = new List<QuoteAccessoryLineDto>();
        decimal productAluminumTotal = 0m;
        decimal productGlassTotal = 0m;
        decimal productAccessoryTotal = 0m;

        foreach (var json in snapshots)
        {
            var snap = TryParseSnapshot(json);
            if (snap is null)
                continue;

            if (snap.Costs is not null)
            {
                productAluminumTotal += snap.Costs.AluminumCost;
                productGlassTotal += snap.Costs.GlassCost;
                productAccessoryTotal += snap.Costs.AccessoryCost;
            }

            if (snap.GlassPanes is not null)
            {
                foreach (var g in snap.GlassPanes)
                {
                    productGlass.Add(new QuoteGlassLineDto(
                        g.GlassSpecId,
                        g.Name,
                        g.WidthMm,
                        g.HeightMm,
                        g.AreaM2PerPane,
                        g.PaneQuantity,
                        g.PricePerSqm,
                        g.LineCost));
                }
            }

            if (snap.AccessoryLines is not null)
            {
                foreach (var a in snap.AccessoryLines)
                {
                    productAccessories.Add(new QuoteAccessoryLineDto(
                        a.AccessoryId,
                        a.Name,
                        a.UnitPrice,
                        a.Quantity,
                        a.LineCost));
                }
            }
        }

        var accessories = sectionAccessories
            .Concat(productAccessories)
            .GroupBy(a => new { a.AccessoryId, a.Name, a.UnitPrice })
            .Select(g =>
            {
                var qty = g.Sum(x => x.Quantity);
                var line = g.Sum(x => x.LineTotal);
                return new QuoteAccessoryLineDto(
                    g.Key.AccessoryId,
                    g.Key.Name,
                    g.Key.UnitPrice,
                    qty,
                    line);
            })
            .OrderBy(a => a.Name)
            .ToList();

        var glassLines = productGlass
            .GroupBy(g => new
            {
                g.GlassSpecId,
                g.Name,
                g.WidthMm,
                g.HeightMm,
                g.AreaM2PerPane,
                g.PricePerSqm
            })
            .Select(g =>
            {
                var qty = g.Sum(x => x.PaneQuantity);
                var line = g.Sum(x => x.LineTotal);
                return new QuoteGlassLineDto(
                    g.Key.GlassSpecId,
                    g.Key.Name,
                    g.Key.WidthMm,
                    g.Key.HeightMm,
                    g.Key.AreaM2PerPane,
                    qty,
                    g.Key.PricePerSqm,
                    line);
            })
            .OrderBy(g => g.Name)
            .ToList();

        // Prefer cutting-plan aluminum when present; otherwise use product snapshot aluminum.
        var materialCost = quote.MaterialCost > 0
            ? quote.MaterialCost
            : productAluminumTotal;

        var productionAreaM2 = ComputeProductionAreaM2(sections);
        var productionCost = ComputeProductionCost(
            productionAreaM2,
            quote.ProductionPricePerM2);

        var surchargesTotal = quote.Surcharges.Sum(s => s.Amount);
        var accessoriesTotal = accessories.Sum(a => a.LineTotal);
        // Prefer aggregated glass lines; fall back to cost totals from snapshots.
        var glassTotal = glassLines.Count > 0
            ? glassLines.Sum(g => g.LineTotal)
            : productGlassTotal;

        return new ClientQuoteDto(
            quote.Id,
            quote.ProjectId,
            quote.ProjectName,
            quote.CustomerName,
            quote.CuttingPlanId,
            quote.WorkshopId,
            quote.WorkshopName,
            quote.MaterialCode,
            quote.MaterialName,
            materialCost,
            productionAreaM2,
            quote.ProductionPricePerM2,
            productionCost,
            accessoriesTotal,
            glassTotal,
            productAluminumTotal,
            surchargesTotal,
            materialCost
            + productionCost
            + accessoriesTotal
            + glassTotal
            + surchargesTotal,
            quote.Notes,
            quote.CreatedAtUtc,
            quote.Surcharges,
            accessories,
            glassLines,
            sections);
    }

    private static ItemCalculationSnapshot? TryParseSnapshot(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ItemCalculationSnapshot>(
                json,
                SnapshotJsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Opening area (m²) from aluminum-sector sections only
    /// (sector linked to a profile material).
    /// </summary>
    internal static decimal ComputeProductionAreaM2(
        IReadOnlyList<QuoteSectionPreviewDto> sections)
    {
        decimal totalArea = 0m;
        foreach (var s in sections)
        {
            if (!s.CountsForProduction)
                continue;

            if (s.HeightMm is int h and > 0
                && s.WidthMm is int w and > 0
                && s.UnitQuantity is int qty and > 0)
            {
                totalArea += (decimal)h * w * qty / 1_000_000m;
            }
        }

        return Math.Round(totalArea, 4, MidpointRounding.AwayFromZero);
    }

    internal static decimal ComputeProductionCost(
        IReadOnlyList<QuoteSectionPreviewDto> sections,
        decimal? productionPricePerM2) =>
        ComputeProductionCost(
            ComputeProductionAreaM2(sections),
            productionPricePerM2);

    internal static decimal ComputeProductionCost(
        decimal productionAreaM2,
        decimal? productionPricePerM2)
    {
        if (productionPricePerM2 is null or <= 0 || productionAreaM2 <= 0)
            return 0m;

        return Math.Round(
            productionAreaM2 * productionPricePerM2.Value,
            2,
            MidpointRounding.AwayFromZero);
    }

    private sealed class ItemCalculationSnapshot
    {
        public SnapshotCosts? Costs { get; set; }
        public List<SnapshotGlassPane>? GlassPanes { get; set; }
        public List<SnapshotAccessoryLine>? AccessoryLines { get; set; }
    }

    private sealed class SnapshotCosts
    {
        public decimal AluminumCost { get; set; }
        public decimal GlassCost { get; set; }
        public decimal AccessoryCost { get; set; }
        public decimal TotalCost { get; set; }
    }

    private sealed class SnapshotGlassPane
    {
        public Guid GlassSpecId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int WidthMm { get; set; }
        public int HeightMm { get; set; }
        public decimal AreaM2PerPane { get; set; }
        public int PaneQuantity { get; set; }
        public decimal PricePerSqm { get; set; }
        public decimal LineCost { get; set; }
    }

    private sealed class SnapshotAccessoryLine
    {
        public Guid AccessoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineCost { get; set; }
    }
}
