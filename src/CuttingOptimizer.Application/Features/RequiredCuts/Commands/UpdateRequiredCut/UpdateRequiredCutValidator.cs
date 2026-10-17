using FluentValidation;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.UpdateRequiredCut;

public sealed class UpdateRequiredCutValidator
    : AbstractValidator<UpdateRequiredCutCommand>
{
    public UpdateRequiredCutValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.ProjectSectionId)
            .NotEmpty();

        RuleFor(x => x.MaterialId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.LengthMm)
            .GreaterThan(0);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}