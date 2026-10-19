namespace CuttingOptimizer.Domain.Entities;

public class ProjectItemAccessoryChoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectItemId { get; set; }

    public Guid TemplateAccessorySlotId { get; set; }

    public Guid AccessoryId { get; set; }

    /// <summary>Override quantity; null = use template rule.</summary>
    public int? QuantityOverride { get; set; }

    public ProjectItem ProjectItem { get; set; } = null!;

    public TemplateAccessorySlot TemplateAccessorySlot { get; set; } = null!;

    public Accessory Accessory { get; set; } = null!;
}