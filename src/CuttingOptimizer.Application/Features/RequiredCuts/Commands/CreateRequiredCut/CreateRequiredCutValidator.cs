using CuttingOptimizer.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Application.Features.RequiredCuts.Commands.CreateRequiredCut;

public sealed class CreateRequiredCutValidator
    : AbstractValidator<CreateRequiredCutCommand>
{
    public CreateRequiredCutValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.ProjectSectionId)
            .NotEmpty()
            .WithMessage(_ => localizer["ProjectSectionRequired"]);

        RuleFor(x => x.MaterialId)
            .NotEmpty()
            .WithMessage(_ => localizer["MaterialRequired"]);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150)
            .WithMessage(_ => localizer["NameRequiredMax150"]);

        RuleFor(x => x.LengthMm)
            .GreaterThan(0)
            .WithMessage(_ => localizer["LengthMustBePositive"]);

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(_ => localizer["QuantityMustBePositive"]);
    }
}
