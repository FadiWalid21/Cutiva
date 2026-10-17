namespace CuttingOptimizer.Domain.Entities;

public class CuttingPlanUnallocatedPiece
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CuttingPlanId { get; set; }

    public Guid RequiredCutId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int LengthMm { get; set; }

    public CuttingPlan CuttingPlan { get; set; } = null!;

    public RequiredCut RequiredCut { get; set; } = null!;
}
