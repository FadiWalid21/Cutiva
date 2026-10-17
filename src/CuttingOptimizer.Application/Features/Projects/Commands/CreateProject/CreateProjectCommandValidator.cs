using FluentValidation;

namespace CuttingOptimizer.Application.Features.Projects;

public sealed class CreateProjectCommandValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.WorkshopId)
            .NotEmpty()
            .WithMessage("Workshop ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required.")
            .MaximumLength(200)
            .WithMessage("Project name must not exceed 200 characters.");

        RuleFor(x => x.CustomerName)
            .MaximumLength(150)
            .WithMessage("Customer name must not exceed 150 characters.");
    }
}