namespace CuttingOptimizer.Domain.Entities;

public class RequiredCut
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectSectionId { get; set; }

    public Guid MaterialId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int LengthMm { get; set; }

    public int Quantity { get; set; }

    public ProjectSection ProjectSection { get; set; } = null!;

    public Material Material { get; set; } = null!;
}