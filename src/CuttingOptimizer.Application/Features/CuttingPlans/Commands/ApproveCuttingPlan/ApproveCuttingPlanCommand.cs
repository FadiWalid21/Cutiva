using CuttingOptimizer.Application.Features.CuttingPlans;
using MediatR;

namespace CuttingOptimizer.Application.Features.CuttingPlans.Commands.ApproveCuttingPlan;

public sealed record ApproveCuttingPlanCommand(Guid CuttingPlanId)
    : IRequest<ApproveCuttingPlanResult>;

public abstract record ApproveCuttingPlanResult;

public sealed record ApproveCuttingPlanSucceeded(CuttingPlanDto Plan)
    : ApproveCuttingPlanResult;

public sealed record ApproveCuttingPlanNotFound : ApproveCuttingPlanResult;

public sealed record ApproveCuttingPlanAlreadyApproved : ApproveCuttingPlanResult;

public sealed record ApproveCuttingPlanInsufficientStock : ApproveCuttingPlanResult;
