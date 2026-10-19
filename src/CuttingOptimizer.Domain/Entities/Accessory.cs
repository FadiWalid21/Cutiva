using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class Accessory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkshopId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public AccessoryUnit Unit { get; set; } = AccessoryUnit.Piece;

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Workshop Workshop { get; set; } = null!;

    public ICollection<ProjectSectionAccessory> SectionLinks { get; set; }
        = new List<ProjectSectionAccessory>();
}
