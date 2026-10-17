namespace CuttingOptimizer.Domain.Entities;


public class Workshop
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public ICollection<Project> Projects { get; set; }
        = new List<Project>();
}