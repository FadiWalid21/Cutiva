using CuttingOptimizer.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Application.Features.Projects;

public sealed class CreateProjectCommandValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.WorkshopId)
            .NotEmpty()
            .WithMessage(_ => localizer["WorkshopIdRequired"]);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_ => localizer["ProjectNameRequired"])
            .MaximumLength(200)
            .WithMessage(_ => localizer["ProjectNameMaxLength"]);

        RuleFor(x => x.CustomerName)
            .MaximumLength(150)
            .WithMessage(_ => localizer["CustomerNameMaxLength"]);
    }
}
