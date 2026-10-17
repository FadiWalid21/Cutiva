namespace CuttingOptimizer.Domain.Entities;

public class CuttingPlanBar
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CuttingPlanId { get; set; }

    public Guid StockBarId { get; set; }

    public int LengthMm { get; set; }

    public decimal UnitPrice { get; set; }

    public int KerfMm { get; set; }

    public int RemainingLengthMm { get; set; }

    public CuttingPlan CuttingPlan { get; set; } = null!;

    public StockBar StockBar { get; set; } = null!;

    public ICollection<CuttingPlanPiece> Pieces { get; set; }
        = new List<CuttingPlanPiece>();
}
