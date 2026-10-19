using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class ProjectItemInspection
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectItemId { get; set; }

    public string? LocationLabel { get; set; }

    public int? MeasuredWidthMm { get; set; }

    public int? MeasuredHeightMm { get; set; }

    public DateTime? InspectedAtUtc { get; set; }

    public string? InspectedBy { get; set; }

    public string? Notes { get; set; }

    public InspectionStatus Status { get; set; } = InspectionStatus.Pending;

    public ProjectItem ProjectItem { get; set; } = null!;
}
