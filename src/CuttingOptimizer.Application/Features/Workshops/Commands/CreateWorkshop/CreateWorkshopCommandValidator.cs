using CuttingOptimizer.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.CreateWorkshop;

public sealed class CreateWorkshopCommandValidator
    : AbstractValidator<CreateWorkshopCommand>
{
    public CreateWorkshopCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_ => localizer["WorkshopNameRequired"])
            .MaximumLength(150)
            .WithMessage(_ => localizer["WorkshopNameMaxLength"]);
    }
}
