using FluentValidation;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.UpdateProjectItem;

public sealed class UpdateProjectItemCommandValidator
    : AbstractValidator<UpdateProjectItemCommand>
{
    public UpdateProjectItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WidthMm).GreaterThan(0);
        RuleFor(x => x.HeightMm).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PartChoices).NotNull();
        RuleFor(x => x.GlassChoices).NotNull();
        RuleFor(x => x.AccessoryChoices).NotNull();
    }
}
