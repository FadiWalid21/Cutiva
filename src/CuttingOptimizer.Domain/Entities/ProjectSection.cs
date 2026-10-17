namespace CuttingOptimizer.Domain.Entities;

public class ProjectSection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }

    public Guid SectorId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;

    public Sector Sector { get; set; } = null!;
    
    public ICollection<RequiredCut> RequiredCuts { get; set; }
        = new List<RequiredCut>();
}