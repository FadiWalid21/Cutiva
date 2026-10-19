export type ProjectStatus = 'optimized' | 'inProgress' | 'incomplete';

export interface DashboardKpis {
  materialEfficiencyPercent: number;
  wastePercent: number;
  stockBarsUsed: number;
  totalMaterialCost: number;
}

export interface RemnantInventoryItem {
  id: string;
  materialId: string;
  materialName: string;
  lengthMm: number;
  quantity: number;
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
  remnantInventory: RemnantInventoryItem[];
  recentProjects: RecentProject[];
}
