using MediatR;

namespace CuttingOptimizer.Application.Features.Reports.Queries.GetWorkshopReport;

public sealed record GetWorkshopReportQuery(
    Guid WorkshopId,
    DateTime FromUtc,
    DateTime ToUtc)
    : IRequest<WorkshopReportDto?>;
