namespace CuttingOptimizer.Domain.Entities;

public class ProjectSection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }

    public Guid SectorId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Opening/frame height in mm (null for legacy length-only rows).</summary>
    public int? HeightMm { get; set; }

    /// <summary>Opening/frame width in mm (null for legacy length-only rows).</summary>
    public int? WidthMm { get; set; }

    /// <summary>How many identical windows/doors at this size.</summary>
    public int? UnitQuantity { get; set; }

    public Project Project { get; set; } = null!;

    public Sector Sector { get; set; } = null!;

    public ICollection<RequiredCut> RequiredCuts { get; set; }
        = new List<RequiredCut>();

    public ICollection<ProjectSectionAccessory> Accessories { get; set; }
        = new List<ProjectSectionAccessory>();
}
