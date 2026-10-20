/** Matches GET /api/workshops and GET /api/workshops/{id} */
export interface WorkshopSummary {
  id: string;
  name: string;
  productionPricePerM2?: number | null;
}

export interface PagedWorkshopsResponse {
  items: WorkshopSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
}

/** Matches GET /api/workshops/{id}/dashboard */
export interface WorkshopDashboardResponse {
  kpis: WorkshopDashboardKpisDto;
  recentProjects: DashboardRecentProjectDto[];
  remnantInventory: RemnantInventoryItemDto[];
}

export interface WorkshopDashboardKpisDto {
  materialEfficiencyPercent: number;
  wastePercent: number;
  stockBarsUsed: number;
  totalMaterialCost: number;
}

export interface DashboardRecentProjectDto {
  projectId: string;
  projectName: string;
  cuttingPlanId: string | null;
  materialName: string | null;
  efficiencyPercent: number;
  isComplete: boolean;
  lastPlanAtUtc: string;
}

export interface RemnantInventoryItemDto {
  stockBarId: string;
  materialId: string;
  materialName: string;
  lengthMm: number;
  availableQuantity: number;
}

/** Matches GET /api/workshops/{id}/reports/summary */
export interface WorkshopReportResponse {
  fromUtc: string;
  toUtc: string;
  kpis: WorkshopReportKpisDto;
  topMaterialsByCost: WorkshopReportMaterialRowDto[];
  projects: WorkshopReportProjectRowDto[];
}

export interface WorkshopReportKpisDto {
  projectsActive: number;
  plansGenerated: number;
  plansApproved: number;
  stockBarsUsed: number;
  remnantBarsInStock: number;
  remnantBarsConsumed: number;
  materialEfficiencyPercent: number;
  wastePercent: number;
  totalMaterialCost: number;
}

export interface WorkshopReportMaterialRowDto {
  materialId: string;
  materialName: string;
  plansApproved: number;
  stockBarsUsed: number;
  totalMaterialCost: number;
  wastePercent: number;
}

export interface WorkshopReportProjectRowDto {
  projectId: string;
  projectName: string;
  plansGenerated: number;
  plansApproved: number;
  totalMaterialCost: number;
  lastActivityUtc: string;
}
