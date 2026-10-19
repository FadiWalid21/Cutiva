using CuttingOptimizer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Workshop> Workshops { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectSection> ProjectSections { get; }
    DbSet<Sector> Sectors { get; }
    DbSet<Material> Materials { get; }
    DbSet<StockBar> StockBars { get; }
    DbSet<RequiredCut> RequiredCuts { get; }
    DbSet<CuttingPlan> CuttingPlans { get; }
    DbSet<CuttingPlanBar> CuttingPlanBars { get; }
    DbSet<CuttingPlanPiece> CuttingPlanPieces { get; }
    DbSet<CuttingPlanUnallocatedPiece> CuttingPlanUnallocatedPieces { get; }
    DbSet<ClientQuote> ClientQuotes { get; }
    DbSet<ClientQuoteSurcharge> ClientQuoteSurcharges { get; }
    DbSet<Accessory> Accessories { get; }
    DbSet<ProjectSectionAccessory> ProjectSectionAccessories { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<FxRateSnapshot> FxRateSnapshots { get; }
    DbSet<WorkshopSubscription> WorkshopSubscriptions { get; }
    DbSet<PasswordResetOtp> PasswordResetOtps { get; }
    DbSet<GlassType> GlassTypes { get; }
    DbSet<GlassSpec> GlassSpecs { get; }
    DbSet<ProductType> ProductTypes { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<TemplatePart> TemplateParts { get; }
    DbSet<TemplatePartAllowedMaterial> TemplatePartAllowedMaterials { get; }
    DbSet<TemplateGlassSlot> TemplateGlassSlots { get; }
    DbSet<TemplateAccessorySlot> TemplateAccessorySlots { get; }
    DbSet<ProjectItem> ProjectItems { get; }
    DbSet<ProjectItemInspection> ProjectItemInspections { get; }
    DbSet<ProjectItemPartChoice> ProjectItemPartChoices { get; }
    DbSet<ProjectItemGlassChoice> ProjectItemGlassChoices { get; }
    DbSet<ProjectItemAccessoryChoice> ProjectItemAccessoryChoices { get; }
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
