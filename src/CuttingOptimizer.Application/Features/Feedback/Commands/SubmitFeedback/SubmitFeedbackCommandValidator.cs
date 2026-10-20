using FluentValidation;

namespace CuttingOptimizer.Application.Features.Feedback.Commands.SubmitFeedback;

public sealed class SubmitFeedbackCommandValidator
    : AbstractValidator<SubmitFeedbackCommand>
{
    public SubmitFeedbackCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(450);
        RuleFor(x => x.OwnerDisplayName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
        RuleFor(x => x.Trigger).IsInEnum();
    }
}
