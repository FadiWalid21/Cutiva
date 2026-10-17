using FluentValidation;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;

public sealed class CreateWorkshopCommandValidator
    : AbstractValidator<CreateWorkshopCommand>
{
    public CreateWorkshopCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Workshop name is required.")
            .MaximumLength(150)
            .WithMessage("Workshop name must not exceed 150 characters.");
    }
}