/** Matches GET /api/workshops and GET /api/workshops/{id} */
export interface WorkshopSummary {
  id: string;
  name: string;
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
  projectedLeftovers: ProjectedLeftoverDto[];
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

export interface ProjectedLeftoverDto {
  cuttingPlanId: string;
  cuttingPlanBarId: string;
  stockBarId: string;
  materialId: string;
  materialName: string;
  remainingLengthMm: number;
  sourceBarLengthMm: number;
  planCreatedAtUtc: string;
  isProjected: boolean;
}
