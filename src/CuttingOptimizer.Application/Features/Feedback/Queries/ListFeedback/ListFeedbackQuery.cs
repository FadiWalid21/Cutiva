using CuttingOptimizer.Application.Features.Feedback;
using MediatR;

namespace CuttingOptimizer.Application.Features.Feedback.Queries.ListFeedback;

public sealed record ListFeedbackQuery(int Take = 100)
    : IRequest<IReadOnlyList<WorkshopFeedbackDto>>;
