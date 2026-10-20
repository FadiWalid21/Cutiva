using System.ComponentModel.DataAnnotations;
using CuttingOptimizer.Domain.Enums;

namespace CuttingOptimizer.Api.Contracts.Feedback;

public sealed record SubmitFeedbackRequest(
    [Required] int Rating,
    string? Comment,
    [Required] FeedbackTrigger Trigger,
    Guid? WorkshopId);
