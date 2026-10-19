namespace CuttingOptimizer.Domain.Entities;

public class TemplatePartAllowedMaterial
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TemplatePartId { get; set; }

    public Guid MaterialId { get; set; }

    public TemplatePart TemplatePart { get; set; } = null!;

    public Material Material { get; set; } = null!;
}
