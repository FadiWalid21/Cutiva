using FluentValidation;

namespace CuttingOptimizer.Application.Features.Glass.Commands.UpdateGlassType;

public sealed class UpdateGlassTypeCommandValidator
    : AbstractValidator<UpdateGlassTypeCommand>
{
    public UpdateGlassTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
