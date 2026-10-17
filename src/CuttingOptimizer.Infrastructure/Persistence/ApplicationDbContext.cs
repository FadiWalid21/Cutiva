
using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Infrastructure.Persistence;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Workshop> Workshops => Set<Workshop>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Sector> Sectors => Set<Sector>();

    public DbSet<ProjectSection> ProjectSections =>
        Set<ProjectSection>();

    public DbSet<Material> Materials => Set<Material>();

    public DbSet<StockBar> StockBars => Set<StockBar>();

    public DbSet<RequiredCut> RequiredCuts => Set<RequiredCut>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureWorkshop(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureSector(modelBuilder);
        ConfigureProjectSection(modelBuilder);
        ConfigureMaterial(modelBuilder);
        ConfigureStockBar(modelBuilder);
        ConfigureRequiredCut(modelBuilder);
    }

    private static void ConfigureWorkshop(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workshop>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(150)
                .IsRequired();
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
        });
    }

    private static void ConfigureProjectSection(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectSection>(entity =>
        {
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

            entity.HasIndex(x => new { x.MaterialId, x.IsActive });

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

            entity.HasIndex(x => new
            {
                x.ProjectSectionId,
                x.MaterialId
            });

            entity.HasOne(x => x.ProjectSection)
                .WithMany(x => x.RequiredCuts)
                .HasForeignKey(x => x.ProjectSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Material)
                .WithMany(x => x.RequiredCuts)
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}