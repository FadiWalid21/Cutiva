using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Domain.Entities;

public class WorkshopFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    public Guid? WorkshopId { get; set; }

    /// <summary>Snapshot of the user display name at submit time.</summary>
    public string OwnerDisplayName { get; set; } = string.Empty;

    /// <summary>Snapshot of the workshop/company name at submit time.</summary>
    public string CompanyName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public FeedbackTrigger Trigger { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Workshop? Workshop { get; set; }
}
