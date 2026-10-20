using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
    public DbSet<Workshop> Workshops => Set<Workshop>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Sector> Sectors => Set<Sector>();

    public DbSet<ProjectSection> ProjectSections =>
        Set<ProjectSection>();

    public DbSet<Material> Materials => Set<Material>();

    public DbSet<StockBar> StockBars => Set<StockBar>();

    public DbSet<RequiredCut> RequiredCuts => Set<RequiredCut>();

    public DbSet<CuttingPlan> CuttingPlans => Set<CuttingPlan>();

    public DbSet<CuttingPlanBar> CuttingPlanBars => Set<CuttingPlanBar>();

    public DbSet<CuttingPlanPiece> CuttingPlanPieces => Set<CuttingPlanPiece>();

    public DbSet<CuttingPlanUnallocatedPiece> CuttingPlanUnallocatedPieces =>
        Set<CuttingPlanUnallocatedPiece>();

    public DbSet<ClientQuote> ClientQuotes => Set<ClientQuote>();

    public DbSet<ClientQuoteSurcharge> ClientQuoteSurcharges =>
        Set<ClientQuoteSurcharge>();

    public DbSet<Accessory> Accessories => Set<Accessory>();

    public DbSet<ProjectSectionAccessory> ProjectSectionAccessories =>
        Set<ProjectSectionAccessory>();

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    public DbSet<FeatureDefinition> FeatureDefinitions => Set<FeatureDefinition>();

    public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures =>
        Set<SubscriptionPlanFeature>();

    public DbSet<FxRateSnapshot> FxRateSnapshots => Set<FxRateSnapshot>();

    public DbSet<WorkshopSubscription> WorkshopSubscriptions =>
        Set<WorkshopSubscription>();

    public DbSet<PasswordResetOtp> PasswordResetOtps => Set<PasswordResetOtp>();

    public DbSet<GlassType> GlassTypes => Set<GlassType>();

    public DbSet<GlassSpec> GlassSpecs => Set<GlassSpec>();

    public DbSet<ProductType> ProductTypes => Set<ProductType>();

    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    public DbSet<TemplatePart> TemplateParts => Set<TemplatePart>();

    public DbSet<TemplatePartAllowedMaterial> TemplatePartAllowedMaterials =>
        Set<TemplatePartAllowedMaterial>();

    public DbSet<TemplateGlassSlot> TemplateGlassSlots => Set<TemplateGlassSlot>();

    public DbSet<TemplateAccessorySlot> TemplateAccessorySlots =>
        Set<TemplateAccessorySlot>();

    public DbSet<ProjectItem> ProjectItems => Set<ProjectItem>();

    public DbSet<ProjectItemInspection> ProjectItemInspections =>
        Set<ProjectItemInspection>();

    public DbSet<ProjectItemPartChoice> ProjectItemPartChoices =>
        Set<ProjectItemPartChoice>();

    public DbSet<ProjectItemGlassChoice> ProjectItemGlassChoices =>
        Set<ProjectItemGlassChoice>();

    public DbSet<ProjectItemAccessoryChoice> ProjectItemAccessoryChoices =>
        Set<ProjectItemAccessoryChoice>();

    public DbSet<WorkshopFeedback> WorkshopFeedbacks => Set<WorkshopFeedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureApplicationUser(modelBuilder);
        ConfigureWorkshop(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureSector(modelBuilder);
        ConfigureProjectSection(modelBuilder);
        ConfigureMaterial(modelBuilder);
        ConfigureStockBar(modelBuilder);
        ConfigureRequiredCut(modelBuilder);
        ConfigureCuttingPlan(modelBuilder);
        ConfigureCuttingPlanBar(modelBuilder);
        ConfigureCuttingPlanPiece(modelBuilder);
        ConfigureCuttingPlanUnallocatedPiece(modelBuilder);
        ConfigureClientQuote(modelBuilder);
        ConfigureClientQuoteSurcharge(modelBuilder);
        ConfigureAccessory(modelBuilder);
        ConfigureProjectSectionAccessory(modelBuilder);
        ConfigureSubscriptionPlan(modelBuilder);
        ConfigurePlanFeatures(modelBuilder);
        ConfigureFxRateSnapshot(modelBuilder);
        ConfigureWorkshopSubscription(modelBuilder);
        ConfigurePasswordResetOtp(modelBuilder);
        ConfigureGlassType(modelBuilder);
        ConfigureGlassSpec(modelBuilder);
        ConfigureProductType(modelBuilder);
        ConfigureProductVariant(modelBuilder);
        ConfigureTemplatePart(modelBuilder);
        ConfigureTemplatePartAllowedMaterial(modelBuilder);
        ConfigureTemplateGlassSlot(modelBuilder);
        ConfigureTemplateAccessorySlot(modelBuilder);
        ConfigureProjectItem(modelBuilder);
        ConfigureProjectItemInspection(modelBuilder);
        ConfigureProjectItemPartChoice(modelBuilder);
        ConfigureProjectItemGlassChoice(modelBuilder);
        ConfigureProjectItemAccessoryChoice(modelBuilder);
        ConfigureWorkshopFeedback(modelBuilder);
    }

    private static void ConfigureApplicationUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.DisplayName)
                .HasMaxLength(150);

            entity.HasIndex(x => x.WorkshopId);
        });
    }

    private static void ConfigureWorkshop(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workshop>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ProductionPricePerM2)
                .HasPrecision(12, 2);
        });
    }

    private static void ConfigureProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.CustomerName)
                .HasMaxLength(150);

            entity.HasOne(x => x.Workshop)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSector(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sector>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            entity.HasIndex(x => x.MaterialId);
        });
    }

    private static void ConfigureProjectSection(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectSection>(entity =>
        {
            entity.ToTable("ProjectSections", table =>
            {
                table.HasCheckConstraint(
                    "CK_ProjectSections_HeightMm",
                    "\"HeightMm\" IS NULL OR \"HeightMm\" > 0");

                table.HasCheckConstraint(
                    "CK_ProjectSections_WidthMm",
                    "\"WidthMm\" IS NULL OR \"WidthMm\" > 0");

                table.HasCheckConstraint(
                    "CK_ProjectSections_UnitQuantity",
                    "\"UnitQuantity\" IS NULL OR \"UnitQuantity\" > 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasOne(x => x.Project)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Sector)
                .WithMany(x => x.ProjectSections)
                .HasForeignKey(x => x.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new
            {
                x.ProjectId,
                x.SectorId
            });
        });
    }

    private static void ConfigureMaterial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("Materials");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.Category)
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasDefaultValue(Domain.Enums.MaterialCategory.Aluminum);

            entity.Property(x => x.ThicknessMm)
                .HasPrecision(10, 2);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => new
            {
                x.SectorId,
                x.Code
            })
            .IsUnique();

            entity.HasOne(x => x.Sector)
                .WithMany()
                .HasForeignKey(x => x.SectorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureStockBar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockBar>(entity =>
        {
            entity.ToTable("StockBars", table =>
            {
                table.HasCheckConstraint(
                    "CK_StockBars_LengthMm",
                    "\"LengthMm\" > 0");

                table.HasCheckConstraint(
                    "CK_StockBars_UnitPrice",
                    "\"UnitPrice\" >= 0");

                table.HasCheckConstraint(
                    "CK_StockBars_AvailableQuantity",
                    "\"AvailableQuantity\" >= 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(12, 2);

            entity.Property(x => x.AvailableQuantity)
                .HasDefaultValue(0);

            entity.Property(x => x.IsRemnant)
                .HasDefaultValue(false);

            entity.HasIndex(x => new { x.MaterialId, x.IsActive });
            entity.HasIndex(x => new { x.MaterialId, x.IsRemnant, x.IsActive });

            entity.HasOne(x => x.Material)
                .WithMany(x => x.StockBars)
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    private static void ConfigureRequiredCut(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequiredCut>(entity =>
        {
            entity.ToTable("RequiredCuts", table =>
            {
                table.HasCheckConstraint(
                    "CK_RequiredCuts_LengthMm",
                    "\"LengthMm\" > 0");

                table.HasCheckConstraint(
                    "CK_RequiredCuts_Quantity",
                    "\"Quantity\" > 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.SourcePartRole)
                .HasMaxLength(64);

            entity.HasIndex(x => new
            {
                x.ProjectSectionId,
                x.MaterialId
            });

            entity.HasIndex(x => x.SourceProjectItemId);

            entity.HasOne(x => x.ProjectSection)
                .WithMany(x => x.RequiredCuts)
                .HasForeignKey(x => x.ProjectSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Material)
                .WithMany(x => x.RequiredCuts)
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SourceProjectItem)
                .WithMany()
                .HasForeignKey(x => x.SourceProjectItemId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }

    private static void ConfigureCuttingPlan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuttingPlan>(entity =>
        {
            entity.ToTable("CuttingPlans", table =>
            {
                table.HasCheckConstraint(
                    "CK_CuttingPlans_KerfMm",
                    "\"KerfMm\" >= 0");

                table.HasCheckConstraint(
                    "CK_CuttingPlans_TotalPieceLengthMm",
                    "\"TotalPieceLengthMm\" >= 0");

                table.HasCheckConstraint(
                    "CK_CuttingPlans_TotalStockLengthMm",
                    "\"TotalStockLengthMm\" >= 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TotalMaterialCost)
                .HasPrecision(12, 2);

            entity.Property(x => x.StrategyNote)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.StrategyCode)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            entity.HasIndex(x => new { x.WorkshopId, x.CreatedAtUtc });
            entity.HasIndex(x => x.ComparisonGroupId);

            entity.HasOne(x => x.Workshop)
                .WithMany()
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCuttingPlanBar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuttingPlanBar>(entity =>
        {
            entity.ToTable("CuttingPlanBars");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(12, 2);

            entity.HasIndex(x => x.CuttingPlanId);

            entity.HasOne(x => x.CuttingPlan)
                .WithMany(x => x.Bars)
                .HasForeignKey(x => x.CuttingPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.StockBar)
                .WithMany()
                .HasForeignKey(x => x.StockBarId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCuttingPlanPiece(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuttingPlanPiece>(entity =>
        {
            entity.ToTable("CuttingPlanPieces");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(x => x.CuttingPlanBarId);

            entity.HasOne(x => x.CuttingPlanBar)
                .WithMany(x => x.Pieces)
                .HasForeignKey(x => x.CuttingPlanBarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RequiredCut)
                .WithMany()
                .HasForeignKey(x => x.RequiredCutId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCuttingPlanUnallocatedPiece(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuttingPlanUnallocatedPiece>(entity =>
        {
            entity.ToTable("CuttingPlanUnallocatedPieces");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(x => x.CuttingPlanId);

            entity.HasOne(x => x.CuttingPlan)
                .WithMany(x => x.UnallocatedPieces)
                .HasForeignKey(x => x.CuttingPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RequiredCut)
                .WithMany()
                .HasForeignKey(x => x.RequiredCutId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureClientQuote(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientQuote>(entity =>
        {
            entity.ToTable("ClientQuotes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.HasIndex(x => new { x.ProjectId, x.CuttingPlanId })
                .IsUnique();

            entity.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.CuttingPlan)
                .WithMany()
                .HasForeignKey(x => x.CuttingPlanId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });
    }

    private static void ConfigureClientQuoteSurcharge(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientQuoteSurcharge>(entity =>
        {
            entity.ToTable("ClientQuoteSurcharges");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Reason)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(x => x.Amount)
                .HasPrecision(18, 2);

            entity.HasIndex(x => x.ClientQuoteId);

            entity.HasOne(x => x.ClientQuote)
                .WithMany(x => x.Surcharges)
                .HasForeignKey(x => x.ClientQuoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAccessory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accessory>(entity =>
        {
            entity.ToTable("Accessories");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Brand)
                .HasMaxLength(100);

            entity.Property(x => x.Unit)
                .HasConversion<string>()
                .HasMaxLength(16)
                .HasDefaultValue(Domain.Enums.AccessoryUnit.Piece);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            entity.HasIndex(x => new { x.WorkshopId, x.Name })
                .IsUnique();

            entity.HasOne(x => x.Workshop)
                .WithMany(x => x.Accessories)
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProjectSectionAccessory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectSectionAccessory>(entity =>
        {
            entity.ToTable("ProjectSectionAccessories");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.ProjectSectionId, x.AccessoryId })
                .IsUnique();

            entity.HasOne(x => x.ProjectSection)
                .WithMany(x => x.Accessories)
                .HasForeignKey(x => x.ProjectSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Accessory)
                .WithMany(x => x.SectionLinks)
                .HasForeignKey(x => x.AccessoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSubscriptionPlan(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("SubscriptionPlans");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(40)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.PriceSar)
                .HasPrecision(12, 2);

            entity.Property(x => x.DurationDays)
                .HasDefaultValue(30);
        });
    }

    private static void ConfigurePlanFeatures(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeatureDefinition>(entity =>
        {
            entity.ToTable("FeatureDefinitions");

            entity.HasKey(x => x.Code);

            entity.Property(x => x.Code)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(120)
                .IsRequired();
        });

        modelBuilder.Entity<SubscriptionPlanFeature>(entity =>
        {
            entity.ToTable("SubscriptionPlanFeatures");

            entity.HasKey(x => new { x.PlanId, x.FeatureCode });

            entity.Property(x => x.FeatureCode)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasOne(x => x.Plan)
                .WithMany(p => p.Features)
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Feature)
                .WithMany(f => f.PlanFeatures)
                .HasForeignKey(x => x.FeatureCode)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureFxRateSnapshot(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FxRateSnapshot>(entity =>
        {
            entity.ToTable("FxRateSnapshots");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FromCurrency)
                .HasMaxLength(8)
                .IsRequired();

            entity.Property(x => x.ToCurrency)
                .HasMaxLength(8)
                .IsRequired();

            entity.Property(x => x.Rate)
                .HasPrecision(18, 8);

            entity.HasIndex(x => new { x.FromCurrency, x.ToCurrency })
                .IsUnique();
        });
    }

    private static void ConfigureWorkshopSubscription(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkshopSubscription>(entity =>
        {
            entity.ToTable("WorkshopSubscriptions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.Property(x => x.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(x => x.PaymentAmount)
                .HasPrecision(12, 2);

            entity.Property(x => x.PaymentCurrency)
                .HasMaxLength(8);

            entity.Property(x => x.PaymentNote)
                .HasMaxLength(500);

            entity.HasIndex(x => x.WorkshopId);
            entity.HasIndex(x => new { x.WorkshopId, x.Status });

            entity.HasOne(x => x.Workshop)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Plan)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePasswordResetOtp(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordResetOtp>(entity =>
        {
            entity.ToTable("PasswordResetOtps");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(x => x.CodeHash)
                .HasMaxLength(128)
                .IsRequired();

            entity.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
        });
    }

    private static void ConfigureGlassType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GlassType>(entity =>
        {
            entity.ToTable("GlassTypes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => new { x.WorkshopId, x.Name })
                .IsUnique();

            entity.HasOne(x => x.Workshop)
                .WithMany(x => x.GlassTypes)
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureGlassSpec(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GlassSpec>(entity =>
        {
            entity.ToTable("GlassSpecs", table =>
            {
                table.HasCheckConstraint(
                    "CK_GlassSpecs_ThicknessMm",
                    "\"ThicknessMm\" > 0");

                table.HasCheckConstraint(
                    "CK_GlassSpecs_PricePerSqm",
                    "\"PricePerSqm\" >= 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ThicknessMm)
                .HasPrecision(10, 2);

            entity.Property(x => x.Description)
                .HasMaxLength(200);

            entity.Property(x => x.PricePerSqm)
                .HasPrecision(18, 2);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => x.GlassTypeId);

            entity.HasOne(x => x.GlassType)
                .WithMany(x => x.Specs)
                .HasForeignKey(x => x.GlassTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProductType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.ToTable("ProductTypes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Kind)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => x.Kind);
        });
    }

    private static void ConfigureProductVariant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("ProductVariants");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => new { x.ProductTypeId, x.Code })
                .IsUnique();

            entity.HasOne(x => x.ProductType)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTemplatePart(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TemplatePart>(entity =>
        {
            entity.ToTable("TemplateParts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.Property(x => x.DisplayName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.WidthCoef)
                .HasPrecision(12, 4);

            entity.Property(x => x.HeightCoef)
                .HasPrecision(12, 4);

            entity.HasIndex(x => x.ProductVariantId);

            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.Parts)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.DefaultAluminumMaterial)
                .WithMany()
                .HasForeignKey(x => x.DefaultAluminumMaterialId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }

    private static void ConfigureTemplatePartAllowedMaterial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TemplatePartAllowedMaterial>(entity =>
        {
            entity.ToTable("TemplatePartAllowedMaterials");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.TemplatePartId, x.MaterialId })
                .IsUnique();

            entity.HasOne(x => x.TemplatePart)
                .WithMany(x => x.AllowedMaterials)
                .HasForeignKey(x => x.TemplatePartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureTemplateGlassSlot(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TemplateGlassSlot>(entity =>
        {
            entity.ToTable("TemplateGlassSlots");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.WidthCoefW).HasPrecision(12, 4);
            entity.Property(x => x.HeightCoefW).HasPrecision(12, 4);
            entity.Property(x => x.WidthCoefH).HasPrecision(12, 4);
            entity.Property(x => x.HeightCoefH).HasPrecision(12, 4);

            entity.HasIndex(x => x.ProductVariantId);

            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.GlassSlots)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTemplateAccessorySlot(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TemplateAccessorySlot>(entity =>
        {
            entity.ToTable("TemplateAccessorySlots");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.QuantityRule)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasIndex(x => x.ProductVariantId);

            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.AccessorySlots)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.DefaultAccessory)
                .WithMany()
                .HasForeignKey(x => x.DefaultAccessoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });
    }

    private static void ConfigureProjectItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectItem>(entity =>
        {
            entity.ToTable("ProjectItems", table =>
            {
                table.HasCheckConstraint(
                    "CK_ProjectItems_WidthMm",
                    "\"WidthMm\" > 0");

                table.HasCheckConstraint(
                    "CK_ProjectItems_HeightMm",
                    "\"HeightMm\" > 0");

                table.HasCheckConstraint(
                    "CK_ProjectItems_Quantity",
                    "\"Quantity\" > 0");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Label)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(x => x.ProjectId);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProjectItemInspection(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectItemInspection>(entity =>
        {
            entity.ToTable("ProjectItemInspections");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LocationLabel)
                .HasMaxLength(150);

            entity.Property(x => x.InspectedBy)
                .HasMaxLength(150);

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasIndex(x => x.ProjectItemId)
                .IsUnique();

            entity.HasOne(x => x.ProjectItem)
                .WithOne(x => x.Inspection)
                .HasForeignKey<ProjectItemInspection>(x => x.ProjectItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProjectItemPartChoice(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectItemPartChoice>(entity =>
        {
            entity.ToTable("ProjectItemPartChoices");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasIndex(x => new { x.ProjectItemId, x.TemplatePartId })
                .IsUnique();

            entity.HasOne(x => x.ProjectItem)
                .WithMany(x => x.PartChoices)
                .HasForeignKey(x => x.ProjectItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.TemplatePart)
                .WithMany()
                .HasForeignKey(x => x.TemplatePartId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProjectItemGlassChoice(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectItemGlassChoice>(entity =>
        {
            entity.ToTable("ProjectItemGlassChoices");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.ProjectItemId, x.TemplateGlassSlotId })
                .IsUnique();

            entity.HasOne(x => x.ProjectItem)
                .WithMany(x => x.GlassChoices)
                .HasForeignKey(x => x.ProjectItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.TemplateGlassSlot)
                .WithMany()
                .HasForeignKey(x => x.TemplateGlassSlotId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.GlassSpec)
                .WithMany()
                .HasForeignKey(x => x.GlassSpecId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProjectItemAccessoryChoice(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectItemAccessoryChoice>(entity =>
        {
            entity.ToTable("ProjectItemAccessoryChoices");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.ProjectItemId, x.TemplateAccessorySlotId })
                .IsUnique();

            entity.HasOne(x => x.ProjectItem)
                .WithMany(x => x.AccessoryChoices)
                .HasForeignKey(x => x.ProjectItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.TemplateAccessorySlot)
                .WithMany()
                .HasForeignKey(x => x.TemplateAccessorySlotId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Accessory)
                .WithMany()
                .HasForeignKey(x => x.AccessoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureWorkshopFeedback(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkshopFeedback>(entity =>
        {
            entity.ToTable("WorkshopFeedbacks");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(x => x.OwnerDisplayName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.CompanyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Comment)
                .HasMaxLength(1000);

            entity.Property(x => x.Trigger)
                .HasConversion<string>()
                .HasMaxLength(32);

            entity.HasIndex(x => x.CreatedAtUtc);
            entity.HasIndex(x => x.UserId);

            entity.HasOne(x => x.Workshop)
                .WithMany()
                .HasForeignKey(x => x.WorkshopId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}