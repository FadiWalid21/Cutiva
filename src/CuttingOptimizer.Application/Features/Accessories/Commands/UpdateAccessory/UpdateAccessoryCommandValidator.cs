using FluentValidation;

namespace CuttingOptimizer.Application.Features.Accessories.Commands.UpdateAccessory;

public sealed class UpdateAccessoryCommandValidator
    : AbstractValidator<UpdateAccessoryCommand>
{
    public UpdateAccessoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}
