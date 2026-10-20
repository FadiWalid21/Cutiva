using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Application.Features.Feedback;

public sealed record WorkshopFeedbackDto(
    Guid Id,
    string UserId,
    Guid? WorkshopId,
    string OwnerDisplayName,
    string CompanyName,
    int Rating,
    string? Comment,
    FeedbackTrigger Trigger,
    DateTime CreatedAtUtc);
