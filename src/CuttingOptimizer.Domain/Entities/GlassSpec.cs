namespace CuttingOptimizer.Domain.Entities;

public class GlassSpec
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GlassTypeId { get; set; }

    /// <summary>Overall thickness in mm (e.g. 24 for 6+12+6).</summary>
    public decimal ThicknessMm { get; set; }

    /// <summary>Optional makeup description, e.g. 6+12+6.</summary>
    public string? Description { get; set; }

    public decimal PricePerSqm { get; set; }

    public bool IsActive { get; set; } = true;

    public GlassType GlassType { get; set; } = null!;
}
