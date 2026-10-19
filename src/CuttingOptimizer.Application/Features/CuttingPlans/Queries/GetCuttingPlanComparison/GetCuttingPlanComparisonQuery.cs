using CuttingOptimizer.Application.Features.CuttingPlans;
using MediatR;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Queries.GetCuttingPlanComparison;

public sealed record GetCuttingPlanComparisonQuery(Guid ComparisonGroupId)
    : IRequest<CuttingPlanComparisonDto?>;
