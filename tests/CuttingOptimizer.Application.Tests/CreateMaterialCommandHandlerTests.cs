using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Materials.Commands.CreateMaterial;
using CuttingOptimizer.Application.Features.Materials.Commands.DeleteMaterial;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CuttingOptimizer.Application.Tests;

public sealed class CreateMaterialCommandHandlerTests
{
    [Fact]
    public async Task Create_NewCode_InsertsActiveMaterial()
    {
        await using var db = MaterialsTestDb.Create();
        var sectorId = await SeedMarketSectorAsync(db);

        var result = await CreateAsync(db, sectorId, "al-01", "Profile A");

        var ok = Assert.IsType<CreateMaterialSucceeded>(result);
        Assert.False(ok.Reactivated);
        Assert.Equal("AL-01", ok.Code);
        var saved = await db.Materials.SingleAsync();
        Assert.True(saved.IsActive);
        Assert.Equal(MaterialCategory.Aluminum, saved.Category);
    }

    [Fact]
    public async Task Create_ActiveDuplicateCode_ReturnsCodeExists()
    {
        await using var db = MaterialsTestDb.Create();
        var sectorId = await SeedMarketSectorAsync(db);
        await CreateAsync(db, sectorId, "AL-01", "Profile A");

        var result = await CreateAsync(db, sectorId, "al-01", "Profile B");

        var conflict = Assert.IsType<CreateMaterialCodeExists>(result);
        Assert.Equal("AL-01", conflict.Code);
        Assert.Equal(1, await db.Materials.CountAsync());
    }

    [Fact]
    public async Task Create_AfterHardDelete_InsertsNewRowWithSameCode()
    {
        await using var db = MaterialsTestDb.Create();
        var sectorId = await SeedMarketSectorAsync(db);
        var first = Assert.IsType<CreateMaterialSucceeded>(
            await CreateAsync(db, sectorId, "AL-01", "Old name"));

        var deleted = await new DeleteMaterialHandler(db)
            .Handle(new DeleteMaterialCommand(first.Id), CancellationToken.None);
        Assert.IsType<DeleteMaterialSucceeded>(deleted);
        Assert.Equal(0, await db.Materials.CountAsync());

        var result = await CreateAsync(db, sectorId, "al-01", "New name", thicknessMm: 1.4m);

        var ok = Assert.IsType<CreateMaterialSucceeded>(result);
        Assert.False(ok.Reactivated);
        Assert.NotEqual(first.Id, ok.Id);
        var saved = await db.Materials.SingleAsync();
        Assert.Equal("New name", saved.Name);
        Assert.Equal(1.4m, saved.ThicknessMm);
    }

    [Fact]
    public async Task Delete_WhenReferencedByRequiredCut_ReturnsInUse()
    {
        await using var db = MaterialsTestDb.Create();
        var sectorId = await SeedMarketSectorAsync(db);
        var created = Assert.IsType<CreateMaterialSucceeded>(
            await CreateAsync(db, sectorId, "AL-01", "In use"));

        db.RequiredCuts.Add(new RequiredCut
        {
            ProjectSectionId = Guid.NewGuid(),
            MaterialId = created.Id,
            Name = "rail",
            LengthMm = 1000,
            Quantity = 1
        });
        await db.SaveChangesAsync();

        var deleted = await new DeleteMaterialHandler(db)
            .Handle(new DeleteMaterialCommand(created.Id), CancellationToken.None);

        Assert.IsType<DeleteMaterialInUse>(deleted);
        Assert.Equal(1, await db.Materials.CountAsync());
    }

    [Fact]
    public async Task Create_UnknownSector_ReturnsSectorNotFound()
    {
        await using var db = MaterialsTestDb.Create();

        var result = await CreateAsync(db, Guid.NewGuid(), "AL-01", "Profile A");

        Assert.IsType<CreateMaterialSectorNotFound>(result);
    }

    private static async Task<Guid> SeedMarketSectorAsync(MaterialsTestDb db)
    {
        var sector = new Sector { Code = "MAT", Name = "Market materials" };
        db.Sectors.Add(sector);
        await db.SaveChangesAsync();
        return sector.Id;
    }

    private static Task<CreateMaterialResult> CreateAsync(
        IApplicationDbContext db,
        Guid sectorId,
        string code,
        string name,
        decimal? thicknessMm = null)
    {
        var handler = new CreateMaterialCommandHandler(
            db,
            NullLogger<CreateMaterialCommandHandler>.Instance);
        return handler.Handle(
            new CreateMaterialCommand(sectorId, code, name, null, MaterialCategory.Aluminum, thicknessMm),
            CancellationToken.None);
    }
}

internal sealed class MaterialsTestDbContext(DbContextOptions<MaterialsTestDbContext> options)
    : DbContext(options)
{
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<StockBar> StockBars => Set<StockBar>();
    public DbSet<RequiredCut> RequiredCuts => Set<RequiredCut>();
    public DbSet<CuttingPlan> CuttingPlans => Set<CuttingPlan>();
    public DbSet<CuttingPlanBar> CuttingPlanBars => Set<CuttingPlanBar>();
    public DbSet<TemplatePart> TemplateParts => Set<TemplatePart>();
    public DbSet<TemplatePartAllowedMaterial> TemplatePartAllowedMaterials => Set<TemplatePartAllowedMaterial>();
    public DbSet<ProjectItemPartChoice> ProjectItemPartChoices => Set<ProjectItemPartChoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sector>(b =>
        {
            b.Ignore(s => s.Material);
            b.Ignore(s => s.ProjectSections);
        });
        modelBuilder.Entity<Material>(b =>
        {
            b.Ignore(m => m.Sector);
            b.Ignore(m => m.RequiredCuts);
            b.Ignore(m => m.StockBars);
        });
        modelBuilder.Entity<StockBar>(b =>
        {
            b.Ignore(s => s.Material);
        });
        modelBuilder.Entity<RequiredCut>(b =>
        {
            b.Ignore(c => c.ProjectSection);
            b.Ignore(c => c.Material);
            b.Ignore(c => c.SourceProjectItem);
        });
        modelBuilder.Entity<CuttingPlan>(b =>
        {
            b.Ignore(p => p.Workshop);
            b.Ignore(p => p.Material);
            b.Ignore(p => p.Project);
            b.Ignore(p => p.Bars);
            b.Ignore(p => p.UnallocatedPieces);
        });
        modelBuilder.Entity<CuttingPlanBar>(entity =>
        {
            entity.Ignore(x => x.CuttingPlan);
            entity.Ignore(x => x.StockBar);
            entity.Ignore(x => x.Pieces);
        });
        modelBuilder.Entity<TemplatePart>(b =>
        {
            b.Ignore(t => t.ProductVariant);
            b.Ignore(t => t.DefaultAluminumMaterial);
            b.Ignore(t => t.AllowedMaterials);
        });
        modelBuilder.Entity<TemplatePartAllowedMaterial>(b =>
        {
            b.Ignore(a => a.TemplatePart);
            b.Ignore(a => a.Material);
        });
        modelBuilder.Entity<ProjectItemPartChoice>(b =>
        {
            b.Ignore(c => c.ProjectItem);
            b.Ignore(c => c.TemplatePart);
            b.Ignore(c => c.Material);
        });
    }
}

internal sealed class MaterialsTestDb : IApplicationDbContext, IAsyncDisposable
{
    private readonly MaterialsTestDbContext context = new(
        new DbContextOptionsBuilder<MaterialsTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public static MaterialsTestDb Create() => new();

    public DbSet<Sector> Sectors => context.Sectors;
    public DbSet<Material> Materials => context.Materials;
    public DbSet<StockBar> StockBars => context.StockBars;
    public DbSet<RequiredCut> RequiredCuts => context.RequiredCuts;
    public DbSet<CuttingPlan> CuttingPlans => context.CuttingPlans;
    public DbSet<CuttingPlanBar> CuttingPlanBars => context.CuttingPlanBars;
    public DbSet<TemplatePart> TemplateParts => context.TemplateParts;
    public DbSet<TemplatePartAllowedMaterial> TemplatePartAllowedMaterials =>
        context.TemplatePartAllowedMaterials;
    public DbSet<ProjectItemPartChoice> ProjectItemPartChoices => context.ProjectItemPartChoices;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => context.DisposeAsync();

    public DbSet<Workshop> Workshops => throw new NotSupportedException();
    public DbSet<Project> Projects => throw new NotSupportedException();
    public DbSet<ProjectSection> ProjectSections => throw new NotSupportedException();
    public DbSet<CuttingPlanPiece> CuttingPlanPieces => throw new NotSupportedException();
    public DbSet<CuttingPlanUnallocatedPiece> CuttingPlanUnallocatedPieces => throw new NotSupportedException();
    public DbSet<ClientQuote> ClientQuotes => throw new NotSupportedException();
    public DbSet<ClientQuoteSurcharge> ClientQuoteSurcharges => throw new NotSupportedException();
    public DbSet<Accessory> Accessories => throw new NotSupportedException();
    public DbSet<ProjectSectionAccessory> ProjectSectionAccessories => throw new NotSupportedException();
    public DbSet<SubscriptionPlan> SubscriptionPlans => throw new NotSupportedException();
    public DbSet<FeatureDefinition> FeatureDefinitions => throw new NotSupportedException();
    public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures => throw new NotSupportedException();
    public DbSet<FxRateSnapshot> FxRateSnapshots => throw new NotSupportedException();
    public DbSet<WorkshopSubscription> WorkshopSubscriptions => throw new NotSupportedException();
    public DbSet<PasswordResetOtp> PasswordResetOtps => throw new NotSupportedException();
    public DbSet<GlassType> GlassTypes => throw new NotSupportedException();
    public DbSet<GlassSpec> GlassSpecs => throw new NotSupportedException();
    public DbSet<ProductType> ProductTypes => throw new NotSupportedException();
    public DbSet<ProductVariant> ProductVariants => throw new NotSupportedException();
    public DbSet<TemplateGlassSlot> TemplateGlassSlots => throw new NotSupportedException();
    public DbSet<TemplateAccessorySlot> TemplateAccessorySlots => throw new NotSupportedException();
    public DbSet<ProjectItem> ProjectItems => throw new NotSupportedException();
    public DbSet<ProjectItemInspection> ProjectItemInspections => throw new NotSupportedException();
    public DbSet<ProjectItemGlassChoice> ProjectItemGlassChoices => throw new NotSupportedException();
    public DbSet<ProjectItemAccessoryChoice> ProjectItemAccessoryChoices => throw new NotSupportedException();
    public DbSet<WorkshopFeedback> WorkshopFeedbacks => throw new NotSupportedException();
}
