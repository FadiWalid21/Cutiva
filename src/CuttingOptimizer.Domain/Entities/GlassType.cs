namespace CuttingOptimizer.Domain.Entities;

public class GlassType
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkshopId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Workshop Workshop { get; set; } = null!;

    public ICollection<GlassSpec> Specs { get; set; }
        = new List<GlassSpec>();
}
