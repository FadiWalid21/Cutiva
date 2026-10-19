namespace CuttingOptimizer.Domain.Entities;

public class ProjectSectionAccessory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectSectionId { get; set; }

    public Guid AccessoryId { get; set; }

    public int Quantity { get; set; } = 1;

    public ProjectSection ProjectSection { get; set; } = null!;

    public Accessory Accessory { get; set; } = null!;
}
