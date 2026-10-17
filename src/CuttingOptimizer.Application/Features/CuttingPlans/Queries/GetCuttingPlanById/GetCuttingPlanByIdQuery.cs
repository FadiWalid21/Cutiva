using CuttingOptimizer.Application.Features.CuttingPlans;
using MediatR;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Queries.GetCuttingPlanById;

public sealed record GetCuttingPlanByIdQuery(Guid Id)
    : IRequest<CuttingPlanDto?>;
