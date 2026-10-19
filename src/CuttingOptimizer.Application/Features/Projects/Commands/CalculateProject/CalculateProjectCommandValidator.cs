using FluentValidation;

namespace CuttingOptimizer.Application.Features.Projects.Commands.CalculateProject;

public sealed class CalculateProjectCommandValidator
    : AbstractValidator<CalculateProjectCommand>
{
    public CalculateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.KerfMm).GreaterThanOrEqualTo(0);
    }
}
