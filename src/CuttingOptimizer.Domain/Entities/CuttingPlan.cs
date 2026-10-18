using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class CuttingPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkshopId { get; set; }

    public Guid MaterialId { get; set; }

    public Guid? ProjectId { get; set; }

    public int KerfMm { get; set; }

    public bool IsComplete { get; set; }

    public CuttingPlanStatus Status { get; set; } = CuttingPlanStatus.Draft;

    public DateTime? ApprovedAtUtc { get; set; }

    public int TotalPieceLengthMm { get; set; }

    public int TotalStockLengthMm { get; set; }

    public int TotalBarsUsed { get; set; }

    public decimal TotalMaterialCost { get; set; }

    public int TotalKerfMm { get; set; }

    public int TotalLeftoverMm { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string StrategyNote { get; set; } = string.Empty;

    public Workshop Workshop { get; set; } = null!;

    public Material Material { get; set; } = null!;

    public Project? Project { get; set; }

    public ICollection<CuttingPlanBar> Bars { get; set; }
        = new List<CuttingPlanBar>();

    public ICollection<CuttingPlanUnallocatedPiece> UnallocatedPieces { get; set; }
        = new List<CuttingPlanUnallocatedPiece>();
}
