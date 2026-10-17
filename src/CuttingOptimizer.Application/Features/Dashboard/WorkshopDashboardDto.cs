namespace CuttingOptimizer.Application.Features.Dashboard;

public sealed record WorkshopDashboardDto(
    WorkshopDashboardKpisDto Kpis,
    IReadOnlyList<DashboardRecentProjectDto> RecentProjects,
    IReadOnlyList<ProjectedLeftoverDto> ProjectedLeftovers);

public sealed record WorkshopDashboardKpisDto(
    double MaterialEfficiencyPercent,
    double WastePercent,
    int StockBarsUsed,
    decimal TotalMaterialCost);

public sealed record DashboardRecentProjectDto(
    Guid ProjectId,
    string ProjectName,
    Guid? CuttingPlanId,
    string? MaterialName,
    double EfficiencyPercent,
    bool IsComplete,
    DateTime LastPlanAtUtc);

/// <summary>
/// Leftover length projected by a saved cutting plan. Not real inventory.
/// </summary>
public sealed record ProjectedLeftoverDto(
    Guid CuttingPlanId,
    Guid CuttingPlanBarId,
    Guid StockBarId,
    Guid MaterialId,
    string MaterialName,
    int RemainingLengthMm,
    int SourceBarLengthMm,
    DateTime PlanCreatedAtUtc,
    bool IsProjected);
