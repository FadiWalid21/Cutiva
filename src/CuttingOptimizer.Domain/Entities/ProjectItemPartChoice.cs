using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class ProjectItemPartChoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectItemId { get; set; }

    public Guid TemplatePartId { get; set; }

    public TemplatePartRole Role { get; set; }

    public Guid MaterialId { get; set; }

    public ProjectItem ProjectItem { get; set; } = null!;

    public TemplatePart TemplatePart { get; set; } = null!;

    public Material Material { get; set; } = null!;
}
