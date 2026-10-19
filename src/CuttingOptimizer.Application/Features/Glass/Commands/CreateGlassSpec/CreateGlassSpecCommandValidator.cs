using FluentValidation;

namespace CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassSpec;

public sealed class CreateGlassSpecCommandValidator
    : AbstractValidator<CreateGlassSpecCommand>
{
    public CreateGlassSpecCommandValidator()
    {
        RuleFor(x => x.GlassTypeId).NotEmpty();
        RuleFor(x => x.ThicknessMm).GreaterThan(0);
        RuleFor(x => x.PricePerSqm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).MaximumLength(200);
    }
}
