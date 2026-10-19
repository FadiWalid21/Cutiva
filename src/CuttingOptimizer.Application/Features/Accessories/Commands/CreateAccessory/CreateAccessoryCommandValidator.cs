using FluentValidation;

namespace CuttingOptimizer.Application.Features.Accessories.Commands.CreateAccessory;

public sealed class CreateAccessoryCommandValidator
    : AbstractValidator<CreateAccessoryCommand>
{
    public CreateAccessoryCommandValidator()
    {
        RuleFor(x => x.WorkshopId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}
