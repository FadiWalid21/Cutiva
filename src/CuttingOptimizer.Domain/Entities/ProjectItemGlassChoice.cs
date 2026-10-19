namespace CuttingOptimizer.Domain.Entities;

public class ProjectItemGlassChoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectItemId { get; set; }

    public Guid TemplateGlassSlotId { get; set; }

    public Guid GlassSpecId { get; set; }

    public ProjectItem ProjectItem { get; set; } = null!;

    public TemplateGlassSlot TemplateGlassSlot { get; set; } = null!;

    public GlassSpec GlassSpec { get; set; } = null!;
}
