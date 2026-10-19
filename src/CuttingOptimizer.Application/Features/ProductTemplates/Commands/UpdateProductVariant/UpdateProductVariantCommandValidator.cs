using FluentValidation;

namespace CuttingOptimizer.Application.Features.ProductTemplates.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantCommandValidator
    : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SashCount)
            .GreaterThan(0)
            .When(x => x.SashCount.HasValue);
        RuleFor(x => x.OverlapMm)
            .GreaterThanOrEqualTo(0)
            .When(x => x.OverlapMm.HasValue);
    }
}
