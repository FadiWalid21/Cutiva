namespace CuttingOptimizer.Domain.Entities;

public class Sector
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICollection<ProjectSection> ProjectSections { get; set; }
        = new List<ProjectSection>();
}