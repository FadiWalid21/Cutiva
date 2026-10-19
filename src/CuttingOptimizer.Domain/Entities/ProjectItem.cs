namespace CuttingOptimizer.Domain.Entities;

public class ProjectItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }

    public Guid ProductVariantId { get; set; }

    public string Label { get; set; } = string.Empty;

    public int WidthMm { get; set; }

    public int HeightMm { get; set; }

    public int Quantity { get; set; } = 1;

    /// <summary>
    /// JSON snapshot of formulas/prices used at last calculate.
    /// Null until first successful calculation.
    /// </summary>
    public string? CalculationSnapshotJson { get; set; }

    public DateTime? LastCalculatedAtUtc { get; set; }

    public Project Project { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;

    public ProjectItemInspection? Inspection { get; set; }

    public ICollection<ProjectItemPartChoice> PartChoices { get; set; }
        = new List<ProjectItemPartChoice>();

    public ICollection<ProjectItemGlassChoice> GlassChoices { get; set; }
        = new List<ProjectItemGlassChoice>();

    public ICollection<ProjectItemAccessoryChoice> AccessoryChoices { get; set; }
        = new List<ProjectItemAccessoryChoice>();
}
