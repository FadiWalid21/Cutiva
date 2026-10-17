using FluentValidation;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;

public sealed class GenerateCuttingPlanValidator
    : AbstractValidator<GenerateCuttingPlanQuery>
{
    public GenerateCuttingPlanValidator()
    {
        RuleFor(x => x.MaterialId)
            .NotEmpty();

        RuleFor(x => x.KerfMm)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RequiredCutIds)
            .NotEmpty();

        RuleForEach(x => x.RequiredCutIds)
            .NotEmpty();
    }
}
