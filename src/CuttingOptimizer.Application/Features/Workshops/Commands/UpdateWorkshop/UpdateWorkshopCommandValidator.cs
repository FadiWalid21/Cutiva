using FluentValidation;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.UpdateWorkshop;

public sealed class UpdateWorkshopCommandValidator
    : AbstractValidator<UpdateWorkshopCommand>
{
    public UpdateWorkshopCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Workshop name is required.")
            .MaximumLength(150)
            .WithMessage("Workshop name must not exceed 150 characters.");
    }
}