using CuttingOptimizer.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace CuttingOptimizer.Application.Features.ProjectSections.Commands.CreateProjectSection;

public sealed class CreateProjectSectionCommandValidator
    : AbstractValidator<CreateProjectSectionCommand>
{
    public CreateProjectSectionCommandValidator(IStringLocalizer<Messages> localizer)
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage(_ => localizer["ProjectIdRequired"]);

        RuleFor(x => x.SectorId)
            .NotEmpty()
            .WithMessage(_ => localizer["SectorIdRequired"]);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(_ => localizer["SectionNameRequired"])
            .MaximumLength(200)
            .WithMessage(_ => localizer["SectionNameMaxLength"]);

        RuleFor(x => x.HeightMm)
            .GreaterThan(0)
            .WithMessage(_ => localizer["SectionHeightRequired"]);

        RuleFor(x => x.WidthMm)
            .GreaterThan(0)
            .WithMessage(_ => localizer["SectionWidthRequired"]);

        RuleFor(x => x.UnitQuantity)
            .GreaterThan(0)
            .WithMessage(_ => localizer["SectionUnitQuantityRequired"]);
    }
}
