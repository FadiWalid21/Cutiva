namespace CuttingOptimizer.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    /// <summary>Number of sashes (sliding). Null when not applicable.</summary>
    public int? SashCount { get; set; }

    /// <summary>Overlap between sashes in mm. Null when not applicable.</summary>
    public int? OverlapMm { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsSeeded { get; set; }

    public ProductType ProductType { get; set; } = null!;

    public ICollection<TemplatePart> Parts { get; set; }
        = new List<TemplatePart>();

    public ICollection<TemplateGlassSlot> GlassSlots { get; set; }
        = new List<TemplateGlassSlot>();

    public ICollection<TemplateAccessorySlot> AccessorySlots { get; set; }
        = new List<TemplateAccessorySlot>();
}
