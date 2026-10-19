using CuttingOptimizer.Application.Services.Costing;
using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Tests;

public sealed class ProjectCostCalculatorTests
{
    private static readonly Guid ItemA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ItemB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid MatFrame = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid MatSash = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid GlassSpecId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid AccId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid PartId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid SlotId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid AccSlotId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    [Fact]
    public void BomExplode_SingleSimpleProduct_ExpectedLengthsAndCosts()
    {
        // Frame top: Length = 1*W + 0*H + 0 = 1000
        // Frame jamb: Length = 0*W + 1*H + 0 = 1200; qty 2
        var item = new CostingProjectItemInput(
            ItemA,
            WidthMm: 1000,
            HeightMm: 1200,
            Quantity: 1,
            SashCount: null,
            OverlapMm: null,
            AluminumParts:
            [
                Aluminum(PartId, TemplatePartRole.FrameTop, "Top", 1, 1m, 0m, 0, false, MatFrame),
                Aluminum(Guid.NewGuid(), TemplatePartRole.FrameJamb, "Jamb", 2, 0m, 1m, 0, false, MatFrame)
            ],
            GlassSlots:
            [
                Glass(SlotId, "Main", paneCount: 1, pricePerSqm: 100m)
            ],
            AccessorySlots:
            [
                new CostingAccessorySlotInput(
                    AccSlotId, "Handle", AccessoryQuantityRule.PerProduct, 1, AccId, 25m)
            ]);

        var result = ProjectCostCalculator.Calculate(
            [item],
            [new MaterialPackingCost(MatFrame, TotalCost: 340m, TotalConsumedLengthMm: 3400)]);

        var top = Assert.Single(result.Bom.AluminumPieces, p => p.Role == TemplatePartRole.FrameTop);
        Assert.Equal(1000, top.LengthMm);
        Assert.Equal(1, top.Quantity);

        var jamb = Assert.Single(result.Bom.AluminumPieces, p => p.Role == TemplatePartRole.FrameJamb);
        Assert.Equal(1200, jamb.LengthMm);
        Assert.Equal(2, jamb.Quantity);

        var glass = Assert.Single(result.Bom.GlassPanes);
        Assert.Equal(1000, glass.WidthMm);
        Assert.Equal(1200, glass.HeightMm);
        Assert.Equal(1.2m, glass.AreaM2PerPane);
        Assert.Equal(120m, glass.LineCost);

        var acc = Assert.Single(result.Bom.Accessories);
        Assert.Equal(1, acc.Quantity);
        Assert.Equal(25m, acc.LineCost);

        // Piece length = 1000 + 2*1200 = 3400 → full packing cost to item
        Assert.Equal(340m, result.TotalAluminumCost);
        Assert.Equal(120m, result.TotalGlassCost);
        Assert.Equal(25m, result.TotalAccessoryCost);
        Assert.Equal(485m, result.GrandTotal);
    }

    [Fact]
    public void Allocate_MultipleItemsSharingMaterial_SharesSumToTotal()
    {
        // Item A: 2000mm; Item B: 3000mm; packing cost 100
        var items = new[]
        {
            new CostingProjectItemInput(
                ItemA, 2000, 1000, 1, null, null,
                [Aluminum(PartId, TemplatePartRole.FrameTop, "A", 1, 1m, 0m, 0, false, MatFrame)],
                [],
                []),
            new CostingProjectItemInput(
                ItemB, 3000, 1000, 1, null, null,
                [Aluminum(Guid.NewGuid(), TemplatePartRole.FrameTop, "B", 1, 1m, 0m, 0, false, MatFrame)],
                [],
                [])
        };

        var result = ProjectCostCalculator.Calculate(
            items,
            [new MaterialPackingCost(MatFrame, 100m, TotalConsumedLengthMm: 6000)]);

        Assert.Equal(2, result.AluminumAllocations.Count);
        Assert.Equal(100m, result.AluminumAllocations.Sum(a => a.AllocatedCost));

        var aCost = result.AluminumAllocations.Single(a => a.ProjectItemId == ItemA).AllocatedCost;
        var bCost = result.AluminumAllocations.Single(a => a.ProjectItemId == ItemB).AllocatedCost;
        Assert.Equal(40m, aCost);
        Assert.Equal(60m, bCost);
    }

    [Fact]
    public void BomExplode_DifferentMaterialsPerRole_KeptSeparate()
    {
        var item = new CostingProjectItemInput(
            ItemA, 1000, 2000, 1, null, null,
            [
                Aluminum(PartId, TemplatePartRole.FrameTop, "Top", 1, 1m, 0m, 0, false, MatFrame),
                Aluminum(Guid.NewGuid(), TemplatePartRole.SashRail, "Rail", 1, 1m, 0m, 0, false, MatSash)
            ],
            [],
            []);

        var result = ProjectCostCalculator.Calculate(
            [item],
            [
                new MaterialPackingCost(MatFrame, 50m, 1000),
                new MaterialPackingCost(MatSash, 80m, 1000)
            ]);

        Assert.Contains(result.Bom.AluminumPieces, p => p.MaterialId == MatFrame && p.LengthMm == 1000);
        Assert.Contains(result.Bom.AluminumPieces, p => p.MaterialId == MatSash && p.LengthMm == 1000);

        Assert.Equal(50m, result.AluminumAllocations.Single(a => a.MaterialId == MatFrame).AllocatedCost);
        Assert.Equal(80m, result.AluminumAllocations.Single(a => a.MaterialId == MatSash).AllocatedCost);
        Assert.Equal(130m, result.TotalAluminumCost);
    }

    [Fact]
    public void BomExplode_DoorWithoutGlass_NoGlassLines()
    {
        var door = new CostingProjectItemInput(
            ItemA, 900, 2100, 1, null, null,
            [
                Aluminum(PartId, TemplatePartRole.FrameTop, "Top", 1, 1m, 0m, 0, false, MatFrame),
                Aluminum(Guid.NewGuid(), TemplatePartRole.FrameJamb, "Jamb", 2, 0m, 1m, 0, false, MatFrame)
            ],
            GlassSlots: [],
            AccessorySlots:
            [
                new CostingAccessorySlotInput(
                    AccSlotId, "Lock", AccessoryQuantityRule.PerProduct, 1, AccId, 40m)
            ]);

        var bom = ProjectCostCalculator.BomExplode([door]);

        Assert.Empty(bom.GlassPanes);
        Assert.Equal(2, bom.AluminumPieces.Count);
        Assert.Single(bom.Accessories);
        Assert.Equal(40m, bom.Accessories[0].LineCost);
    }

    [Fact]
    public void BomExplode_DoorWithGlass_IncludesPaneCost()
    {
        var door = new CostingProjectItemInput(
            ItemA, 900, 2100, 1, null, null,
            [
                Aluminum(PartId, TemplatePartRole.FrameTop, "Top", 1, 1m, 0m, 0, false, MatFrame)
            ],
            [
                new CostingGlassSlotInput(
                    SlotId, "Door glass", 1,
                    WidthCoefW: 1m, HeightCoefW: 0m, OffsetWMm: -50,
                    WidthCoefH: 0m, HeightCoefH: 1m, OffsetHMm: -100,
                    UseSashWidthDivision: false,
                    GlassSpecId, PricePerSqm: 200m)
            ],
            []);

        var bom = ProjectCostCalculator.BomExplode([door]);
        var pane = Assert.Single(bom.GlassPanes);

        Assert.Equal(850, pane.WidthMm);
        Assert.Equal(2000, pane.HeightMm);
        Assert.Equal(1.7m, pane.AreaM2PerPane);
        Assert.Equal(340m, pane.LineCost);
    }

    [Fact]
    public void BomExplode_NegativeOffsetProducingNonPositiveLength_Throws()
    {
        var item = new CostingProjectItemInput(
            ItemA, 100, 100, 1, null, null,
            [
                Aluminum(PartId, TemplatePartRole.FrameTop, "Bad", 1, 1m, 0m, OffsetMm: -200, false, MatFrame)
            ],
            [],
            []);

        var ex = Assert.Throws<ProjectCostingValidationException>(
            () => ProjectCostCalculator.BomExplode([item]));

        Assert.Contains("must be greater than zero", ex.Message);
    }

    [Fact]
    public void ComputeGlassAreaM2_ExactDecimal()
    {
        // 1234 × 5678 / 1_000_000 = 7.006652 exactly
        var area = ProjectCostCalculator.ComputeGlassAreaM2(1234, 5678);
        Assert.Equal(7.006652m, area);

        // Classic 1000×1000 → 1.0
        Assert.Equal(1.0m, ProjectCostCalculator.ComputeGlassAreaM2(1000, 1000));
    }

    [Fact]
    public void BomExplode_UseSashWidthDivision_AppliesEffectiveWidth()
    {
        // W=2000, SashCount=2, Overlap=20 → effectiveW = 1000+20 = 1020
        var item = new CostingProjectItemInput(
            ItemA, 2000, 1500, 1, SashCount: 2, OverlapMm: 20,
            [
                Aluminum(PartId, TemplatePartRole.SashRail, "Rail", 1, 1m, 0m, 0, true, MatSash)
            ],
            [],
            [
                new CostingAccessorySlotInput(
                    AccSlotId, "Wheel", AccessoryQuantityRule.PerSash, 2, AccId, 10m)
            ]);

        var bom = ProjectCostCalculator.BomExplode([item]);

        Assert.Equal(1020, Assert.Single(bom.AluminumPieces).LengthMm);
        // PerSash: 2 * 2 sashes * 1 item = 4
        Assert.Equal(4, Assert.Single(bom.Accessories).Quantity);
        Assert.Equal(40m, bom.Accessories[0].LineCost);
    }

    private static CostingAluminumPartInput Aluminum(
        Guid id,
        TemplatePartRole role,
        string name,
        int qtyPerUnit,
        decimal widthCoef,
        decimal heightCoef,
        int OffsetMm,
        bool useSash,
        Guid materialId) =>
        new(id, role, name, qtyPerUnit, widthCoef, heightCoef, OffsetMm, useSash, materialId);

    private static CostingGlassSlotInput Glass(
        Guid id,
        string name,
        int paneCount,
        decimal pricePerSqm) =>
        new(
            id, name, paneCount,
            WidthCoefW: 1m, HeightCoefW: 0m, OffsetWMm: 0,
            WidthCoefH: 0m, HeightCoefH: 1m, OffsetHMm: 0,
            UseSashWidthDivision: false,
            GlassSpecId, pricePerSqm);
}
