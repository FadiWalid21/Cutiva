using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CuttingOptimizer.Infrastructure.Persistence;

/// <summary>
/// Seeds catalog product types/variants with placeholder aluminum formulas.
/// Coefficients are examples only — workshops should edit via the templates API.
/// </summary>
public static class ProductTemplateSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>()
            .CreateLogger("ProductTemplateSeeder");
        var db = sp.GetRequiredService<ApplicationDbContext>();

        if (await db.ProductTypes.AnyAsync())
        {
            logger.LogDebug("ProductTypes already present; skipping seed.");
            return;
        }

        logger.LogInformation(
            "Seeding product templates (placeholder formulas; IsSeeded=true).");

        db.ProductTypes.Add(BuildWindowType());
        db.ProductTypes.Add(BuildDoorType());
        await db.SaveChangesAsync();
    }

    private static ProductType BuildWindowType()
    {
        var type = new ProductType
        {
            Kind = ProductKind.Window,
            Name = "Window",
            IsActive = true,
            IsSeeded = true
        };

        type.Variants.Add(BuildSlidingVariant(type));
        type.Variants.Add(BuildCasementVariant(type));
        type.Variants.Add(BuildTiltVariant(type));
        return type;
    }

    private static ProductType BuildDoorType()
    {
        var type = new ProductType
        {
            Kind = ProductKind.Door,
            Name = "Door",
            IsActive = true,
            IsSeeded = true
        };

        type.Variants.Add(BuildDoorAluminumOnlyVariant(type));
        type.Variants.Add(BuildDoorAluminumGlassVariant(type));
        return type;
    }

    /// <summary>
    /// Sliding: SashCount=2, OverlapMm=25. Placeholder parts use sash-width division
    /// for sash rails/stiles (effectiveW = W/2 + 25).
    /// </summary>
    private static ProductVariant BuildSlidingVariant(ProductType type)
    {
        var variant = NewVariant(
            type,
            name: "Sliding",
            code: "WIN-SLIDING",
            sashCount: 2,
            overlapMm: 25);

        AddStandardFrameParts(variant, useSashWidthOnSash: true);
        AddSashParts(variant, useSashWidthDivision: true, includeInterlock: true);
        AddMainGlassSlot(variant, useSashWidthDivision: true);
        AddHandleAccessorySlot(variant);
        return variant;
    }

    private static ProductVariant BuildCasementVariant(ProductType type)
    {
        var variant = NewVariant(type, "Casement", "WIN-CASEMENT");
        AddStandardFrameParts(variant, useSashWidthOnSash: false);
        AddSashParts(variant, useSashWidthDivision: false, includeInterlock: false);
        AddMainGlassSlot(variant, useSashWidthDivision: false);
        AddHandleAccessorySlot(variant);
        return variant;
    }

    private static ProductVariant BuildTiltVariant(ProductType type)
    {
        var variant = NewVariant(type, "Tilt", "WIN-TILT");
        AddStandardFrameParts(variant, useSashWidthOnSash: false);
        AddSashParts(variant, useSashWidthDivision: false, includeInterlock: false);
        AddMainGlassSlot(variant, useSashWidthDivision: false);
        AddHandleAccessorySlot(variant);
        return variant;
    }

    private static ProductVariant BuildDoorAluminumOnlyVariant(ProductType type)
    {
        var variant = NewVariant(type, "Aluminum Only", "DOOR-ALU");
        AddStandardFrameParts(variant, useSashWidthOnSash: false);
        AddSashParts(variant, useSashWidthDivision: false, includeInterlock: false);
        AddHandleAccessorySlot(variant);
        // No glass slots by design.
        return variant;
    }

    private static ProductVariant BuildDoorAluminumGlassVariant(ProductType type)
    {
        var variant = NewVariant(type, "Aluminum + Glass", "DOOR-ALU-GLASS");
        AddStandardFrameParts(variant, useSashWidthOnSash: false);
        AddSashParts(variant, useSashWidthDivision: false, includeInterlock: false);
        AddMainGlassSlot(variant, useSashWidthDivision: false);
        AddHandleAccessorySlot(variant);
        return variant;
    }

    private static ProductVariant NewVariant(
        ProductType type,
        string name,
        string code,
        int? sashCount = null,
        int? overlapMm = null) =>
        new()
        {
            ProductType = type,
            Name = name,
            Code = code,
            SashCount = sashCount,
            OverlapMm = overlapMm,
            IsActive = true,
            IsSeeded = true
        };

    /// <summary>
    /// Placeholder frame formulas:
    /// Top/Bottom = 1×W; Jambs = 1×H × qty 2.
    /// </summary>
    private static void AddStandardFrameParts(
        ProductVariant variant,
        bool useSashWidthOnSash)
    {
        _ = useSashWidthOnSash; // reserved for future frame/sash split refinements

        variant.Parts.Add(Part(
            variant,
            TemplatePartRole.FrameTop,
            "Frame top (placeholder)",
            qty: 1,
            widthCoef: 1m,
            heightCoef: 0m,
            offsetMm: 0,
            useSashWidth: false));

        variant.Parts.Add(Part(
            variant,
            TemplatePartRole.FrameBottom,
            "Frame bottom (placeholder)",
            qty: 1,
            widthCoef: 1m,
            heightCoef: 0m,
            offsetMm: 0,
            useSashWidth: false));

        variant.Parts.Add(Part(
            variant,
            TemplatePartRole.FrameJamb,
            "Frame jamb (placeholder)",
            qty: 2,
            widthCoef: 0m,
            heightCoef: 1m,
            offsetMm: 0,
            useSashWidth: false));
    }

    /// <summary>
    /// Placeholder sash formulas: stiles = 1×H × 2; rails = 1×W (or sash-width) × 2;
    /// optional interlock stile = 1×H.
    /// </summary>
    private static void AddSashParts(
        ProductVariant variant,
        bool useSashWidthDivision,
        bool includeInterlock)
    {
        variant.Parts.Add(Part(
            variant,
            TemplatePartRole.SashStile,
            "Sash stile (placeholder)",
            qty: 2,
            widthCoef: 0m,
            heightCoef: 1m,
            offsetMm: -40,
            useSashWidth: false));

        variant.Parts.Add(Part(
            variant,
            TemplatePartRole.SashRail,
            "Sash rail (placeholder)",
            qty: 2,
            widthCoef: 1m,
            heightCoef: 0m,
            offsetMm: -40,
            useSashWidth: useSashWidthDivision));

        if (includeInterlock)
        {
            variant.Parts.Add(Part(
                variant,
                TemplatePartRole.InterlockStile,
                "Interlock stile (placeholder)",
                qty: 1,
                widthCoef: 0m,
                heightCoef: 1m,
                offsetMm: -40,
                useSashWidth: false));
        }
    }

    private static void AddMainGlassSlot(
        ProductVariant variant,
        bool useSashWidthDivision)
    {
        variant.GlassSlots.Add(new TemplateGlassSlot
        {
            ProductVariant = variant,
            Name = "Main pane",
            PaneCount = 1,
            WidthCoefW = 1m,
            HeightCoefW = 0m,
            OffsetWMm = -80,
            WidthCoefH = 0m,
            HeightCoefH = 1m,
            OffsetHMm = -80,
            Required = true,
            UseSashWidthDivision = useSashWidthDivision
        });
    }

    private static void AddHandleAccessorySlot(ProductVariant variant)
    {
        variant.AccessorySlots.Add(new TemplateAccessorySlot
        {
            ProductVariant = variant,
            Name = "Handle",
            QuantityRule = AccessoryQuantityRule.PerProduct,
            Qty = 1,
            DefaultAccessoryId = null
        });
    }

    private static TemplatePart Part(
        ProductVariant variant,
        TemplatePartRole role,
        string displayName,
        int qty,
        decimal widthCoef,
        decimal heightCoef,
        int offsetMm,
        bool useSashWidth) =>
        new()
        {
            ProductVariant = variant,
            Role = role,
            DisplayName = displayName,
            QtyPerUnit = qty,
            WidthCoef = widthCoef,
            HeightCoef = heightCoef,
            OffsetMm = offsetMm,
            UseSashWidthDivision = useSashWidth,
            DefaultAluminumMaterialId = null
        };
}
