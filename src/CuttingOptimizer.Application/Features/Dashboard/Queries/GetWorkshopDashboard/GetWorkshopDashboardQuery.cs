using CuttingOptimizer.Application.Features.Dashboard;
using MediatR;

namespace CuttingOptimizer.Application.Features.Dashboard.Queries.GetWorkshopDashboard;

public sealed record GetWorkshopDashboardQuery(Guid WorkshopId)
    : IRequest<WorkshopDashboardDto?>;
