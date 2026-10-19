using CuttingOptimizer.Domain.Enums;
using FluentValidation;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.UpsertProjectItemInspection;

public sealed class UpsertProjectItemInspectionCommandValidator
    : AbstractValidator<UpsertProjectItemInspectionCommand>
{
    public UpsertProjectItemInspectionCommandValidator()
    {
        RuleFor(x => x.ProjectItemId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.MeasuredWidthMm)
            .GreaterThan(0)
            .When(x => x.MeasuredWidthMm.HasValue);
        RuleFor(x => x.MeasuredHeightMm)
            .GreaterThan(0)
            .When(x => x.MeasuredHeightMm.HasValue);
        RuleFor(x => x.LocationLabel).MaximumLength(150);
        RuleFor(x => x.InspectedBy).MaximumLength(150);
        RuleFor(x => x.Notes).MaximumLength(1000);

        RuleFor(x => x)
            .Must(x => x.MeasuredWidthMm.HasValue && x.MeasuredHeightMm.HasValue)
            .When(x => x.Status == InspectionStatus.Done)
            .WithMessage("Measured dimensions are required when inspection status is Done.");
    }
}
