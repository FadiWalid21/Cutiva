using System.Text.Json;
using System.Text.Json.Serialization;
using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Abstractions.Optimization;
using CuttingOptimizer.Application.Features.ProjectItems;
using CuttingOptimizer.Application.Services.Costing;
using CuttingOptimizer.Application.Services.Optimization;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Projects.Commands.CalculateProject;

public sealed class CalculateProjectHandler(
    IApplicationDbContext context,
    ICuttingPlanOptimizer optimizer)
    : IRequestHandler<CalculateProjectCommand, CalculateProjectCommandResult>
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<CalculateProjectCommandResult> Handle(
        CalculateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
            return new CalculateProjectNotFound();

        var items = await context.ProjectItems
            .Include(i => i.Inspection)
            .Include(i => i.ProductVariant)
                .ThenInclude(v => v.Parts)
            .Include(i => i.ProductVariant)
                .ThenInclude(v => v.GlassSlots)
            .Include(i => i.ProductVariant)
                .ThenInclude(v => v.AccessorySlots)
            .Include(i => i.PartChoices)
                .ThenInclude(c => c.Material)
            .Include(i => i.GlassChoices)
                .ThenInclude(c => c.GlassSpec)
            .Include(i => i.GlassChoices)
                .ThenInclude(c => c.TemplateGlassSlot)
            .Include(i => i.AccessoryChoices)
                .ThenInclude(c => c.Accessory)
            .Include(i => i.AccessoryChoices)
                .ThenInclude(c => c.TemplateAccessorySlot)
            .Where(i => i.ProjectId == request.ProjectId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            return new CalculateProjectNoItems();

        var costingInputs = new List<CostingProjectItemInput>(items.Count);
        var effectiveDims = new Dictionary<Guid, (int W, int H)>();
        var labels = items.ToDictionary(i => i.Id, i => i.Label);

        foreach (var item in items)
        {
            var build = TryBuildCostingInput(item);
            if (build.Error is { } err)
                return new CalculateProjectFailed(err.Code, err.Message);

            costingInputs.Add(build.Input!);
            effectiveDims[item.Id] = (build.Input!.WidthMm, build.Input.HeightMm);
        }

        BomExplodeResult bom;
        try
        {
            bom = ProjectCostCalculator.BomExplode(costingInputs);
        }
        catch (ProjectCostingValidationException ex)
        {
            return new CalculateProjectFailed("InvalidFormula", ex.Message);
        }

        var materialIds = bom.AluminumPieces
            .Select(p => p.MaterialId)
            .Distinct()
            .ToList();

        var materials = await context.Materials
            .AsNoTracking()
            .Where(m => materialIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        var stockBars = await context.StockBars
            .AsNoTracking()
            .Where(s => materialIds.Contains(s.MaterialId)
                        && s.IsActive
                        && (s.IsRemnant ? s.AvailableQuantity > 0 : s.UnitPrice > 0))
            .ToListAsync(cancellationToken);

        if (!request.UseRemnants)
            stockBars = stockBars.Where(s => !s.IsRemnant).ToList();

        var packingCosts = new List<MaterialPackingCost>();
        var purchaseBars = new List<PurchaseBarLineDto>();
        var unallocated = new List<CalculateUnallocatedDto>();
        var waste = new List<CalculateMaterialWasteDto>();
        var piecesByMaterial = bom.AluminumPieces
            .GroupBy(p => p.MaterialId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Synthetic cut ids for optimizer (persisted RequiredCuts created after packing).
        var syntheticCuts = new List<(Guid CutId, Guid ProjectItemId, BomAluminumPiece Piece)>();

        foreach (var (materialId, pieces) in piecesByMaterial)
        {
            var cuts = new List<OptimizationRequiredCut>();
            foreach (var piece in pieces)
            {
                var cutId = Guid.NewGuid();
                syntheticCuts.Add((cutId, piece.ProjectItemId, piece));
                cuts.Add(new OptimizationRequiredCut(
                    cutId,
                    piece.DisplayName,
                    piece.LengthMm,
                    piece.Quantity));
            }

            var options = stockBars
                .Where(s => s.MaterialId == materialId)
                .Select(s => new OptimizationStockOption(
                    s.Id,
                    s.LengthMm,
                    s.IsRemnant ? 0m : s.UnitPrice,
                    s.IsRemnant ? s.AvailableQuantity : int.MaxValue / 4,
                    s.IsRemnant))
                .ToList();

            if (options.Count == 0)
            {
                var code = materials.TryGetValue(materialId, out var m)
                    ? m.Code
                    : materialId.ToString("N")[..8];
                return new CalculateProjectFailed(
                    "NoStock",
                    $"No active stock bars for material {code}.");
            }

            var optResult = optimizer.Optimize(new CuttingOptimizationInput(
                request.KerfMm,
                cuts,
                options,
                BarOpenPreference.CheapestUnitPrice,
                AllowedCatalogStockBarIds: null,
                UseRemnants: request.UseRemnants));

            packingCosts.Add(new MaterialPackingCost(
                materialId,
                optResult.TotalMaterialCost,
                optResult.Bars.Sum(b => b.LengthMm)));

            var material = materials[materialId];
            var stockById = stockBars
                .Where(s => s.MaterialId == materialId)
                .ToDictionary(s => s.Id);

            var barGroups = optResult.Bars
                .GroupBy(b =>
                {
                    var isRemnant = stockById.TryGetValue(b.StockBarId, out var sb)
                                    && sb.IsRemnant;
                    return (b.LengthMm, b.UnitPrice, IsRemnant: isRemnant);
                });

            foreach (var group in barGroups)
            {
                var qty = group.Count();
                purchaseBars.Add(new PurchaseBarLineDto(
                    materialId,
                    material.Code,
                    material.Name,
                    group.Key.LengthMm,
                    qty,
                    group.Key.UnitPrice,
                    group.Key.UnitPrice * qty,
                    group.Key.IsRemnant));
            }

            var unallocGroups = optResult.UnallocatedPieces
                .GroupBy(u => (u.Name, u.LengthMm));
            foreach (var group in unallocGroups)
            {
                unallocated.Add(new CalculateUnallocatedDto(
                    materialId,
                    material.Code,
                    group.Key.Name,
                    group.Key.LengthMm,
                    group.Count()));
            }

            var totalStock = optResult.Bars.Sum(b => b.LengthMm);
            var wastePercent = totalStock <= 0
                ? 0m
                : (decimal)(optResult.TotalKerfMm + optResult.TotalLeftoverMm)
                  * 100m
                  / totalStock;

            waste.Add(new CalculateMaterialWasteDto(
                materialId,
                material.Code,
                optResult.TotalKerfMm,
                optResult.TotalLeftoverMm,
                Math.Round(wastePercent, 2),
                optResult.IsComplete));
        }

        var costResult = ProjectCostCalculator.Calculate(costingInputs, packingCosts);

        // Persist RequiredCuts (BR-9): one generated section per item.
        var cutsError = await ReplaceGeneratedCutsAsync(
            items,
            syntheticCuts,
            materials,
            cancellationToken);

        if (cutsError is not null)
            return new CalculateProjectFailed(cutsError.Value.Code, cutsError.Value.Message);

        var calculatedAt = DateTime.UtcNow;
        foreach (var item in items)
        {
            var input = costingInputs.First(c => c.ProjectItemId == item.Id);
            var summary = costResult.ItemSummaries.First(s => s.ProjectItemId == item.Id);
            var glassForItem = costResult.Bom.GlassPanes
                .Where(g => g.ProjectItemId == item.Id)
                .ToList();
            var accessoriesForItem = costResult.Bom.Accessories
                .Where(a => a.ProjectItemId == item.Id)
                .ToList();
            var snapshot = BuildSnapshot(
                item,
                input,
                summary,
                glassForItem,
                accessoriesForItem);
            item.CalculationSnapshotJson = JsonSerializer.Serialize(snapshot, SnapshotJsonOptions);
            item.LastCalculatedAtUtc = calculatedAt;
        }

        await context.SaveChangesAsync(cancellationToken);

        var itemCosts = costResult.ItemSummaries
            .Select(s =>
            {
                var dims = effectiveDims[s.ProjectItemId];
                return new CalculateItemCostDto(
                    s.ProjectItemId,
                    labels[s.ProjectItemId],
                    dims.W,
                    dims.H,
                    s.AluminumCost,
                    s.GlassCost,
                    s.AccessoryCost,
                    s.TotalCost);
            })
            .OrderBy(i => i.Label)
            .ToList();

        var glassDtos = costResult.Bom.GlassPanes
            .Select(g => new CalculateGlassPaneDto(
                g.ProjectItemId,
                labels[g.ProjectItemId],
                g.GlassSpecId,
                g.Name,
                g.WidthMm,
                g.HeightMm,
                g.AreaM2PerPane,
                g.PaneQuantity,
                g.PricePerSqm,
                g.LineCost))
            .ToList();

        var accessoryDtos = costResult.Bom.Accessories
            .Select(a => new CalculateAccessoryLineDto(
                a.ProjectItemId,
                labels[a.ProjectItemId],
                a.AccessoryId,
                a.Name,
                a.Quantity,
                a.UnitPrice,
                a.LineCost))
            .ToList();

        return new CalculateProjectSucceeded(new CalculateProjectResultDto(
            request.ProjectId,
            costResult.TotalAluminumCost,
            costResult.TotalGlassCost,
            costResult.TotalAccessoryCost,
            costResult.GrandTotal,
            purchaseBars,
            glassDtos,
            accessoryDtos,
            itemCosts,
            unallocated,
            waste));
    }

    private async Task<(string Code, string Message)?> ReplaceGeneratedCutsAsync(
        List<ProjectItem> items,
        List<(Guid CutId, Guid ProjectItemId, BomAluminumPiece Piece)> syntheticCuts,
        Dictionary<Guid, Material> materials,
        CancellationToken cancellationToken)
    {
        var itemIds = items.Select(i => i.Id).ToList();

        var oldCuts = await context.RequiredCuts
            .Where(c => c.SourceProjectItemId != null
                        && itemIds.Contains(c.SourceProjectItemId.Value))
            .ToListAsync(cancellationToken);

        var oldSectionIds = oldCuts
            .Select(c => c.ProjectSectionId)
            .Distinct()
            .ToList();
        var removedCutIds = oldCuts.Select(c => c.Id).ToHashSet();

        context.RequiredCuts.RemoveRange(oldCuts);

        foreach (var sectionId in oldSectionIds)
        {
            var hasOther = await context.RequiredCuts.AnyAsync(
                c => c.ProjectSectionId == sectionId
                     && !removedCutIds.Contains(c.Id),
                cancellationToken);

            if (hasOther)
                continue;

            var section = await context.ProjectSections
                .FirstOrDefaultAsync(s => s.Id == sectionId, cancellationToken);
            if (section is not null)
                context.ProjectSections.Remove(section);
        }

        var projectId = items[0].ProjectId;
        var fallbackSectorId = await context.ProjectSections
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .Select(s => (Guid?)s.SectorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fallbackSectorId is null)
        {
            fallbackSectorId = await context.Sectors
                .AsNoTracking()
                .OrderBy(s => s.Code)
                .Select(s => (Guid?)s.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var itemsById = items.ToDictionary(i => i.Id);
        var cutsByItem = syntheticCuts.GroupBy(c => c.ProjectItemId);

        foreach (var group in cutsByItem)
        {
            var item = itemsById[group.Key];
            var firstPiece = group.First().Piece;
            var sectorId = materials.TryGetValue(firstPiece.MaterialId, out var mat)
                ? mat.SectorId
                : fallbackSectorId;

            if (sectorId is null)
            {
                return ("NoSector",
                    "Cannot resolve a Sector for generated ProjectSections. Create a sector first.");
            }

            var section = new ProjectSection
            {
                ProjectId = projectId,
                SectorId = sectorId.Value,
                Name = $"{item.Label} — generated",
                WidthMm = item.WidthMm,
                HeightMm = item.HeightMm,
                UnitQuantity = item.Quantity
            };

            context.ProjectSections.Add(section);

            foreach (var (cutId, _, piece) in group)
            {
                context.RequiredCuts.Add(new RequiredCut
                {
                    Id = cutId,
                    ProjectSection = section,
                    MaterialId = piece.MaterialId,
                    Name = piece.DisplayName,
                    LengthMm = piece.LengthMm,
                    Quantity = piece.Quantity,
                    SourceProjectItemId = item.Id,
                    SourcePartRole = piece.Role.ToString()
                });
            }
        }

        return null;
    }

    private static (CostingProjectItemInput? Input, (string Code, string Message)? Error)
        TryBuildCostingInput(ProjectItem item)
    {
        var variant = item.ProductVariant;
        var (widthMm, heightMm) = ProjectItemMapper.ResolveEffectiveDimensions(item);

        var accessoryById = variant.AccessorySlots.ToDictionary(s => s.Id);
        var partChoiceByPart = item.PartChoices.ToDictionary(c => c.TemplatePartId);

        var aluminumParts = new List<CostingAluminumPartInput>();
        foreach (var part in variant.Parts)
        {
            if (!partChoiceByPart.TryGetValue(part.Id, out var choice))
            {
                return (null, ("MissingPartChoice",
                    $"Item '{item.Label}' is missing a material choice for part '{part.DisplayName}'."));
            }

            if (choice.Material.Category != MaterialCategory.Aluminum || !choice.Material.IsActive)
            {
                return (null, ("InvalidAluminumMaterial",
                    $"Material for part '{part.DisplayName}' on item '{item.Label}' is not active aluminum."));
            }

            aluminumParts.Add(new CostingAluminumPartInput(
                part.Id,
                part.Role,
                part.DisplayName,
                part.QtyPerUnit,
                part.WidthCoef,
                part.HeightCoef,
                part.OffsetMm,
                part.UseSashWidthDivision,
                choice.MaterialId));
        }

        var glassSlots = new List<CostingGlassSlotInput>();
        foreach (var slot in variant.GlassSlots)
        {
            var choice = item.GlassChoices
                .FirstOrDefault(c => c.TemplateGlassSlotId == slot.Id);

            if (choice is null)
            {
                if (slot.Required)
                {
                    return (null, ("RequiredGlassSlotMissing",
                        $"Item '{item.Label}' is missing glass for required slot '{slot.Name}'."));
                }

                continue;
            }

            glassSlots.Add(new CostingGlassSlotInput(
                slot.Id,
                slot.Name,
                slot.PaneCount,
                slot.WidthCoefW,
                slot.HeightCoefW,
                slot.OffsetWMm,
                slot.WidthCoefH,
                slot.HeightCoefH,
                slot.OffsetHMm,
                slot.UseSashWidthDivision,
                choice.GlassSpecId,
                choice.GlassSpec.PricePerSqm));
        }

        var accessorySlots = new List<CostingAccessorySlotInput>();
        foreach (var choice in item.AccessoryChoices)
        {
            if (!accessoryById.TryGetValue(choice.TemplateAccessorySlotId, out var slot))
            {
                return (null, ("AccessorySlotNotInVariant",
                    $"Item '{item.Label}' has an accessory choice for an unknown slot."));
            }

            var qty = choice.QuantityOverride ?? slot.Qty;
            accessorySlots.Add(new CostingAccessorySlotInput(
                slot.Id,
                slot.Name,
                slot.QuantityRule,
                qty,
                choice.AccessoryId,
                choice.Accessory.UnitPrice));
        }

        return (new CostingProjectItemInput(
            item.Id,
            widthMm,
            heightMm,
            item.Quantity,
            variant.SashCount,
            variant.OverlapMm,
            aluminumParts,
            glassSlots,
            accessorySlots), null);
    }

    private static object BuildSnapshot(
        ProjectItem item,
        CostingProjectItemInput input,
        ProjectItemCostSummary summary,
        IReadOnlyList<BomGlassPane> glassPanes,
        IReadOnlyList<BomAccessoryLine> accessories) =>
        new
        {
            CalculatedAtUtc = DateTime.UtcNow,
            NominalWidthMm = item.WidthMm,
            NominalHeightMm = item.HeightMm,
            EffectiveWidthMm = input.WidthMm,
            EffectiveHeightMm = input.HeightMm,
            Quantity = item.Quantity,
            SashCount = input.SashCount,
            OverlapMm = input.OverlapMm,
            Costs = new
            {
                summary.AluminumCost,
                summary.GlassCost,
                summary.AccessoryCost,
                summary.TotalCost
            },
            AluminumParts = input.AluminumParts.Select(p => new
            {
                p.TemplatePartId,
                Role = p.Role.ToString(),
                p.DisplayName,
                p.QtyPerUnit,
                p.WidthCoef,
                p.HeightCoef,
                p.OffsetMm,
                p.UseSashWidthDivision,
                p.MaterialId
            }),
            GlassSlots = input.GlassSlots.Select(s => new
            {
                s.TemplateGlassSlotId,
                s.Name,
                s.PaneCount,
                s.WidthCoefW,
                s.HeightCoefW,
                s.OffsetWMm,
                s.WidthCoefH,
                s.HeightCoefH,
                s.OffsetHMm,
                s.UseSashWidthDivision,
                s.GlassSpecId,
                s.PricePerSqm
            }),
            GlassPanes = glassPanes.Select(g => new
            {
                g.TemplateGlassSlotId,
                g.GlassSpecId,
                g.Name,
                g.WidthMm,
                g.HeightMm,
                g.AreaM2PerPane,
                g.PaneQuantity,
                g.PricePerSqm,
                g.LineCost
            }),
            AccessorySlots = input.AccessorySlots.Select(s => new
            {
                s.TemplateAccessorySlotId,
                s.Name,
                QuantityRule = s.QuantityRule.ToString(),
                s.Qty,
                s.AccessoryId,
                s.UnitPrice
            }),
            AccessoryLines = accessories.Select(a => new
            {
                a.TemplateAccessorySlotId,
                a.AccessoryId,
                a.Name,
                a.Quantity,
                a.UnitPrice,
                a.LineCost
            })
        };
}
