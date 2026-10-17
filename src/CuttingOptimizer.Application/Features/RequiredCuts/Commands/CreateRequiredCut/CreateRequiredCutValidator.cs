using FluentValidation;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.CreateRequiredCut;

public sealed class CreateRequiredCutValidator
    : AbstractValidator<CreateRequiredCutCommand>
{
    public CreateRequiredCutValidator()
    {
        RuleFor(x => x.ProjectSectionId)
            .NotEmpty()
            .WithMessage("Project section is required.");

        RuleFor(x => x.MaterialId)
            .NotEmpty()
            .WithMessage("Material is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage("Name is required and cannot exceed 150 characters.");

        RuleFor(x => x.LengthMm)
            .GreaterThan(0)
            .WithMessage("Length must be greater than zero.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");
    }
}