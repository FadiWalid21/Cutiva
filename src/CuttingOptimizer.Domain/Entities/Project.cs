namespace CuttingOptimizer.Domain.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkshopId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? CustomerName { get; set; }

    public Workshop Workshop { get; set; } = null!;

    public ICollection<ProjectSection> Sections { get; set; }
        = new List<ProjectSection>();
}