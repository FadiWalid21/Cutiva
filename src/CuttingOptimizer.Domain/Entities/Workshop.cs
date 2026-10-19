namespace CuttingOptimizer.Domain.Entities;

public class Workshop
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    /// <summary>Labor/production rate per m² of opening area. Null or 0 = no production charge.</summary>
    public decimal? ProductionPricePerM2 { get; set; }

    public ICollection<Project> Projects { get; set; }
        = new List<Project>();

    public ICollection<Accessory> Accessories { get; set; }
        = new List<Accessory>();

    public ICollection<GlassType> GlassTypes { get; set; }
        = new List<GlassType>();

    public ICollection<WorkshopSubscription> Subscriptions { get; set; }
        = new List<WorkshopSubscription>();
}
