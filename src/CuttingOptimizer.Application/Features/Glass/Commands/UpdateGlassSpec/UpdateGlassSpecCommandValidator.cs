using FluentValidation;

namespace CuttingOptimizer.Application.Features.Glass.Commands.UpdateGlassSpec;

public sealed class UpdateGlassSpecCommandValidator
    : AbstractValidator<UpdateGlassSpecCommand>
{
    public UpdateGlassSpecCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ThicknessMm).GreaterThan(0);
        RuleFor(x => x.PricePerSqm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).MaximumLength(200);
    }
}
