namespace CuttingOptimizer.Domain.Entities;

public class StockBar
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MaterialId { get; set; }

    public int LengthMm { get; set; }

    public decimal UnitPrice { get; set; }

    public int AvailableQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public Material Material { get; set; } = null!;
}