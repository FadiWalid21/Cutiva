namespace CuttingOptimizer.Domain.Entities;

public class Material
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SectorId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Sector Sector { get; set; } = null!;

    public ICollection<StockBar> StockBars { get; set; }
        = new List<StockBar>();
    
    public ICollection<RequiredCut> RequiredCuts { get; set; }
        = new List<RequiredCut>();
}