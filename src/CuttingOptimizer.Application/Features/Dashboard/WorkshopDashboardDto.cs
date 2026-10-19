namespace CuttingOptimizer.Application.Features.Dashboard;

public sealed record WorkshopDashboardDto(
    WorkshopDashboardKpisDto Kpis,
    IReadOnlyList<DashboardRecentProjectDto> RecentProjects,
    IReadOnlyList<RemnantInventoryItemDto> RemnantInventory);

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
/// Usable remnant stock from approved cutting plans.
/// </summary>
public sealed record RemnantInventoryItemDto(
    Guid StockBarId,
    Guid MaterialId,
    string MaterialName,
    int LengthMm,
    int AvailableQuantity);
