namespace CuttingOptimizer.Domain.Entities;

public class CuttingPlanPiece
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CuttingPlanBarId { get; set; }

    public Guid RequiredCutId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int LengthMm { get; set; }

    public CuttingPlanBar CuttingPlanBar { get; set; } = null!;

    public RequiredCut RequiredCut { get; set; } = null!;
}
