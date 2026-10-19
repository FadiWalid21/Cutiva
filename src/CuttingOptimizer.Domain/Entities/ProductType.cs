using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class ProductType
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public ProductKind Kind { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsSeeded { get; set; }

    public ICollection<ProductVariant> Variants { get; set; }
        = new List<ProductVariant>();
}
