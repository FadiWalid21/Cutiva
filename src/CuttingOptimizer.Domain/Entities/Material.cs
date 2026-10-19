using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class Material
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SectorId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Resource category. Existing rows backfilled to Aluminum.</summary>
    public MaterialCategory Category { get; set; } = MaterialCategory.Aluminum;

    /// <summary>Profile thickness in mm (aluminum). Null when unknown.</summary>
    public decimal? ThicknessMm { get; set; }

    public bool IsActive { get; set; } = true;

    public Sector Sector { get; set; } = null!;

    public ICollection<StockBar> StockBars { get; set; }
        = new List<StockBar>();

    public ICollection<RequiredCut> RequiredCuts { get; set; }
        = new List<RequiredCut>();
}
