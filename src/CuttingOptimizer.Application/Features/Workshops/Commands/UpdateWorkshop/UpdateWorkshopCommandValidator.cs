using CuttingOptimizer.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Application.Features.Workshops.Commands.UpdateWorkshop;

public sealed class UpdateWorkshopCommandValidator
    : AbstractValidator<UpdateWorkshopCommand>
{
    public UpdateWorkshopCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_ => localizer["WorkshopNameRequired"])
            .MaximumLength(150)
            .WithMessage(_ => localizer["WorkshopNameMaxLength"]);
    }
}
