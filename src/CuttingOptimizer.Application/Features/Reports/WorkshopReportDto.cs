namespace CuttingOptimizer.Application.Features.Reports;

public sealed record WorkshopReportDto(
    DateTime FromUtc,
    DateTime ToUtc,
    WorkshopReportKpisDto Kpis,
    IReadOnlyList<WorkshopReportMaterialRowDto> TopMaterialsByCost,
    IReadOnlyList<WorkshopReportProjectRowDto> Projects);

public sealed record WorkshopReportKpisDto(
    int ProjectsActive,
    int PlansGenerated,
    int PlansApproved,
    int StockBarsUsed,
    int RemnantBarsInStock,
    int RemnantBarsConsumed,
    double MaterialEfficiencyPercent,
    double WastePercent,
    decimal TotalMaterialCost);

public sealed record WorkshopReportMaterialRowDto(
    Guid MaterialId,
    string MaterialName,
    int PlansApproved,
    int StockBarsUsed,
    decimal TotalMaterialCost,
    double WastePercent);

public sealed record WorkshopReportProjectRowDto(
    Guid ProjectId,
    string ProjectName,
    int PlansGenerated,
    int PlansApproved,
    decimal TotalMaterialCost,
    DateTime LastActivityUtc);
