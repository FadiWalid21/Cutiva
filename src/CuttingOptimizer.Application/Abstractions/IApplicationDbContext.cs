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
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}