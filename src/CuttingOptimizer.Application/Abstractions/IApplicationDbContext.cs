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
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}