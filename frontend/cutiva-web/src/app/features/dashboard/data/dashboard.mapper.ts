import {
  DashboardRecentProjectDto,
  RemnantInventoryItemDto,
  WorkshopDashboardResponse,
} from '../../../core/api/workshops.models';
import {
  DashboardSnapshot,
  RemnantInventoryItem,
  RecentProject,
} from './dashboard.models';

export function mapWorkshopDashboard(
  response: WorkshopDashboardResponse,
): DashboardSnapshot {
  return {
    kpis: {
      materialEfficiencyPercent: response.kpis.materialEfficiencyPercent,
      wastePercent: response.kpis.wastePercent,
      stockBarsUsed: response.kpis.stockBarsUsed,
      totalMaterialCost: response.kpis.totalMaterialCost,
    },
    remnantInventory: (response.remnantInventory ?? []).map(mapRemnant),
    recentProjects: response.recentProjects.map(mapRecentProject),
  };
}

function mapRemnant(item: RemnantInventoryItemDto): RemnantInventoryItem {
  return {
    id: item.stockBarId,
    materialId: item.materialId,
    materialName: item.materialName,
    lengthMm: item.lengthMm,
    quantity: item.availableQuantity,
  };
}

function mapRecentProject(item: DashboardRecentProjectDto): RecentProject {
  return {
    id: item.projectId,
    name: item.projectName,
    cuttingPlanId: item.cuttingPlanId,
    material: item.materialName ?? '—',
    efficiencyPercent: item.efficiencyPercent,
    status: item.isComplete ? 'optimized' : 'incomplete',
    lastPlanAtUtc: item.lastPlanAtUtc,
  };
}
