using FluentValidation;

namespace CuttingOptimizer.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandValidator
    : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.WorkshopId)
            .NotEmpty();

        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CustomerName)
            .MaximumLength(150);
    }
}