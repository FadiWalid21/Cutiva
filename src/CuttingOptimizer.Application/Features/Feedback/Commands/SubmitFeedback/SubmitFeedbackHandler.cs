using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Feedback;
using CuttingOptimizer.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Feedback.Commands.SubmitFeedback;

public sealed class SubmitFeedbackHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitFeedbackCommand, SubmitFeedbackResult>
{
    public async Task<SubmitFeedbackResult> Handle(
        SubmitFeedbackCommand request,
        CancellationToken cancellationToken)
    {
        string companyName = string.Empty;
        if (request.WorkshopId is Guid workshopId)
        {
            var workshop = await context.Workshops
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workshopId, cancellationToken);

            if (workshop is null)
                return new SubmitFeedbackWorkshopNotFound();

            companyName = workshop.Name;
        }

        var feedback = new WorkshopFeedback
        {
            UserId = request.UserId,
            WorkshopId = request.WorkshopId,
            OwnerDisplayName = request.OwnerDisplayName.Trim(),
            CompanyName = string.IsNullOrWhiteSpace(companyName)
                ? "—"
                : companyName.Trim(),
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment)
                ? null
                : request.Comment.Trim(),
            Trigger = request.Trigger,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.WorkshopFeedbacks.Add(feedback);
        await context.SaveChangesAsync(cancellationToken);

        return new SubmitFeedbackSucceeded(ToDto(feedback));
    }

    internal static WorkshopFeedbackDto ToDto(WorkshopFeedback f) =>
        new(
            f.Id,
            f.UserId,
            f.WorkshopId,
            f.OwnerDisplayName,
            f.CompanyName,
            f.Rating,
            f.Comment,
            f.Trigger,
            f.CreatedAtUtc);
}
