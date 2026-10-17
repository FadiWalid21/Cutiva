using FluentValidation;

namespace CuttingOptimizer.Application.Features.Projects.Commands.DeleteProject;

public sealed class DeleteProjectCommandValidator
    : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.WorkshopId).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}