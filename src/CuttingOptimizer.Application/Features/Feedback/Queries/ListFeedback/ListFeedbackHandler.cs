using CuttingOptimizer.Application.Abstractions;
using CuttingOptimizer.Application.Features.Feedback;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Application.Features.Feedback.Queries.ListFeedback;

public sealed class ListFeedbackHandler(IApplicationDbContext context)
    : IRequestHandler<ListFeedbackQuery, IReadOnlyList<WorkshopFeedbackDto>>
{
    public async Task<IReadOnlyList<WorkshopFeedbackDto>> Handle(
        ListFeedbackQuery request,
        CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, 500);

        return await context.WorkshopFeedbacks
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedAtUtc)
            .Take(take)
            .Select(f => new WorkshopFeedbackDto(
                f.Id,
                f.UserId,
                f.WorkshopId,
                f.OwnerDisplayName,
                f.CompanyName,
                f.Rating,
                f.Comment,
                f.Trigger,
                f.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
