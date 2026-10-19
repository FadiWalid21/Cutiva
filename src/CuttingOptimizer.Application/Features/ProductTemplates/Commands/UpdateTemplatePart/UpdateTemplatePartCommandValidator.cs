using FluentValidation;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateTemplatePart;

public sealed class UpdateTemplatePartCommandValidator
    : AbstractValidator<UpdateTemplatePartCommand>
{
    public UpdateTemplatePartCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.QtyPerUnit).GreaterThan(0);
    }
}
