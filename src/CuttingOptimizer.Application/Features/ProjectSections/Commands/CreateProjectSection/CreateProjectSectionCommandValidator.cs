using FluentValidation;

namespace CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;

public sealed class CreateProjectSectionCommandValidator
    : AbstractValidator<CreateProjectSectionCommand>
{
    public CreateProjectSectionCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID is required.");

        RuleFor(x => x.SectorId)
            .NotEmpty()
            .WithMessage("Sector ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Section name is required.")
            .MaximumLength(200)
            .WithMessage("Section name must not exceed 200 characters.");
    }
}