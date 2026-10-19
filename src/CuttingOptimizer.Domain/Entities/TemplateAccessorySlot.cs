using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class TemplateAccessorySlot
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductVariantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public AccessoryQuantityRule QuantityRule { get; set; }
        = AccessoryQuantityRule.PerProduct;

    public int Qty { get; set; } = 1;

    public Guid? DefaultAccessoryId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    public Accessory? DefaultAccessory { get; set; }
}
