using CuttingOptimizer.Application.Features.CuttingPlans;
using MediatR;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Queries.GenerateCuttingPlan;

public sealed record GenerateCuttingPlanQuery(
    Guid MaterialId,
    int KerfMm,
    IReadOnlyList<Guid> RequiredCutIds,
    bool UseRemnants = true
) : IRequest<CuttingPlanComparisonDto?>;
