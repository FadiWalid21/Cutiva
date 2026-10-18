export type ProjectStatus = 'optimized' | 'inProgress';

export interface DashboardKpis {
  materialEfficiencyPercent: number;
  wastePercent: number;
  stockBarsUsed: number;
  totalMaterialCost: number;
}

export interface ProjectedLeftoverItem {
  id: string;
  cuttingPlanId: string;
  lengthMm: number;
  percentOfBar: number;
  sourceBarLabel: string;
  materialName: string;
  isProjected: boolean;
}

export interface RecentProject {
  id: string;
  name: string;
  cuttingPlanId: string | null;
  material: string;
  efficiencyPercent: number;
  status: ProjectStatus;
  lastPlanAtUtc: string;
}

export interface DashboardSnapshot {
  kpis: DashboardKpis;
  projectedLeftovers: ProjectedLeftoverItem[];
  recentProjects: RecentProject[];
}
