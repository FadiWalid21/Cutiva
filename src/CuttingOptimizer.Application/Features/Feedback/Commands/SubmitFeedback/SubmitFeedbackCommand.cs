using CuttingOptimizer.Application.Features.Feedback;
using CuttingOptimizer.Domain.Enums;
using MediatR;

namespace CuttingOptimizer.Application.Features.Feedback.Commands.SubmitFeedback;

public sealed record SubmitFeedbackCommand(
    string UserId,
    string OwnerDisplayName,
    Guid? WorkshopId,
    int Rating,
    string? Comment,
    FeedbackTrigger Trigger)
    : IRequest<SubmitFeedbackResult>;

public abstract record SubmitFeedbackResult;

public sealed record SubmitFeedbackSucceeded(WorkshopFeedbackDto Feedback)
    : SubmitFeedbackResult;

public sealed record SubmitFeedbackWorkshopNotFound : SubmitFeedbackResult;
