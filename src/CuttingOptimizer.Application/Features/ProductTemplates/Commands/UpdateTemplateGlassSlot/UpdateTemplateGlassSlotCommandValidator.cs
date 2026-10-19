using FluentValidation;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplateGlassSlot;

public sealed class UpdateTemplateGlassSlotCommandValidator
    : AbstractValidator<UpdateTemplateGlassSlotCommand>
{
    public UpdateTemplateGlassSlotCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PaneCount).GreaterThan(0);
    }
}
