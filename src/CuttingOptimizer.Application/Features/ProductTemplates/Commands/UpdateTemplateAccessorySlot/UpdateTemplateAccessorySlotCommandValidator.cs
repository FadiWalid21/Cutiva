using FluentValidation;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplateAccessorySlot;

public sealed class UpdateTemplateAccessorySlotCommandValidator
    : AbstractValidator<UpdateTemplateAccessorySlotCommand>
{
    public UpdateTemplateAccessorySlotCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Qty).GreaterThan(0);
        RuleFor(x => x.QuantityRule).IsInEnum();
    }
}
