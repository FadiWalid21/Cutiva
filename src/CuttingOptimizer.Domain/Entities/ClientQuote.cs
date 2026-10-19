namespace CuttingOptimizer.Domain.Entities;

public class ClientQuote
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }

    /// <summary>Null for glass/accessories-only quotes with no aluminum cutting plan.</summary>
    public Guid? CuttingPlanId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    public Project Project { get; set; } = null!;

    public CuttingPlan? CuttingPlan { get; set; }

    public ICollection<ClientQuoteSurcharge> Surcharges { get; set; }
        = new List<ClientQuoteSurcharge>();
}
