namespace CuttingOptimizer.Domain.Entities;

public class Sector
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Profile material this project element type is made from (null for system MAT bucket).
    /// </summary>
    public Guid? MaterialId { get; set; }

    public Material? Material { get; set; }

    public ICollection<ProjectSection> ProjectSections { get; set; }
        = new List<ProjectSection>();
}
