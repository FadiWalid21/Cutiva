using FluentValidation;

namespace CuttingOptimizer.Application.Features.Glass.Commands.CreateGlassType;

public sealed class CreateGlassTypeCommandValidator
    : AbstractValidator<CreateGlassTypeCommand>
{
    public CreateGlassTypeCommandValidator()
    {
        RuleFor(x => x.WorkshopId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
