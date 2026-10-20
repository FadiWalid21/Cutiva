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
    public async Task Create_AfterSoftDelete_ReactivatesSameRowWithNewValues()
    {
        await using var db = MaterialsTestDb.Create();
        var sectorId = await SeedMarketSectorAsync(db);
        var first = Assert.IsType<CreateMaterialSucceeded>(
            await CreateAsync(db, sectorId, "AL-01", "Old name"));

        var deleted = await new DeleteMaterialHandler(db)
            .Handle(new DeleteMaterialCommand(first.Id), CancellationToken.None);
        Assert.IsType<DeleteMaterialSucceeded>(deleted);
        Assert.False((await db.Materials.SingleAsync()).IsActive);

        var result = await CreateAsync(db, sectorId, "al-01", "New name", thicknessMm: 1.4m);

        var ok = Assert.IsType<CreateMaterialSucceeded>(result);
        Assert.True(ok.Reactivated);
        Assert.Equal(first.Id, ok.Id);
        var saved = await db.Materials.SingleAsync();
        Assert.True(saved.IsActive);
        Assert.Equal("New name", saved.Name);
        Assert.Equal(1.4m, saved.ThicknessMm);
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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => context.DisposeAsync();

    public DbSet<Workshop> Workshops => throw new NotSupportedException();
    public DbSet<Project> Projects => throw new NotSupportedException();
    public DbSet<ProjectSection> ProjectSections => throw new NotSupportedException();
    public DbSet<RequiredCut> RequiredCuts => throw new NotSupportedException();
    public DbSet<CuttingPlan> CuttingPlans => throw new NotSupportedException();
    public DbSet<CuttingPlanBar> CuttingPlanBars => throw new NotSupportedException();
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
    public DbSet<TemplatePart> TemplateParts => throw new NotSupportedException();
    public DbSet<TemplatePartAllowedMaterial> TemplatePartAllowedMaterials => throw new NotSupportedException();
    public DbSet<TemplateGlassSlot> TemplateGlassSlots => throw new NotSupportedException();
    public DbSet<TemplateAccessorySlot> TemplateAccessorySlots => throw new NotSupportedException();
    public DbSet<ProjectItem> ProjectItems => throw new NotSupportedException();
    public DbSet<ProjectItemInspection> ProjectItemInspections => throw new NotSupportedException();
    public DbSet<ProjectItemPartChoice> ProjectItemPartChoices => throw new NotSupportedException();
    public DbSet<ProjectItemGlassChoice> ProjectItemGlassChoices => throw new NotSupportedException();
    public DbSet<ProjectItemAccessoryChoice> ProjectItemAccessoryChoices => throw new NotSupportedException();
    public DbSet<WorkshopFeedback> WorkshopFeedbacks => throw new NotSupportedException();
}
