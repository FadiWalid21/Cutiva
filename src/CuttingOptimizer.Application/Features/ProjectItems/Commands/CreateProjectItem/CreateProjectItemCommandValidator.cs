using FluentValidation;

namespace CuttingOptimizer.Application.Features.ProjectItems.Commands.CreateProjectItem;

public sealed class CreateProjectItemCommandValidator
    : AbstractValidator<CreateProjectItemCommand>
{
    public CreateProjectItemCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ProductVariantId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WidthMm).GreaterThan(0);
        RuleFor(x => x.HeightMm).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PartChoices).NotNull();
        RuleFor(x => x.GlassChoices).NotNull();
        RuleFor(x => x.AccessoryChoices).NotNull();
        RuleForEach(x => x.PartChoices).ChildRules(c =>
        {
            c.RuleFor(x => x.TemplatePartId).NotEmpty();
            c.RuleFor(x => x.MaterialId).NotEmpty();
        });
        RuleForEach(x => x.GlassChoices).ChildRules(c =>
        {
            c.RuleFor(x => x.TemplateGlassSlotId).NotEmpty();
            c.RuleFor(x => x.GlassSpecId).NotEmpty();
        });
        RuleForEach(x => x.AccessoryChoices).ChildRules(c =>
        {
            c.RuleFor(x => x.TemplateAccessorySlotId).NotEmpty();
            c.RuleFor(x => x.AccessoryId).NotEmpty();
            c.RuleFor(x => x.QuantityOverride)
                .GreaterThan(0)
                .When(x => x.QuantityOverride.HasValue);
        });
    }
}
