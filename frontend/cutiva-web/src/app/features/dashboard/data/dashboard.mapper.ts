import {
  DashboardRecentProjectDto,
  ProjectedLeftoverDto,
  WorkshopDashboardResponse,
} from '../../../core/api/workshops.models';
import {
  DashboardSnapshot,
  ProjectedLeftoverItem,
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
    projectedLeftovers: response.projectedLeftovers.map(mapProjectedLeftover),
    recentProjects: response.recentProjects.map(mapRecentProject),
  };
}

function mapProjectedLeftover(item: ProjectedLeftoverDto): ProjectedLeftoverItem {
  const percentOfBar =
    item.sourceBarLengthMm > 0
      ? (item.remainingLengthMm / item.sourceBarLengthMm) * 100
      : 0;

  return {
    id: item.cuttingPlanBarId,
    cuttingPlanId: item.cuttingPlanId,
    lengthMm: item.remainingLengthMm,
    percentOfBar: Math.round(percentOfBar * 10) / 10,
    sourceBarLabel: `${item.sourceBarLengthMm} mm`,
    materialName: item.materialName,
    isProjected: item.isProjected,
  };
}

function mapRecentProject(item: DashboardRecentProjectDto): RecentProject {
  return {
    id: item.projectId,
    name: item.projectName,
    cuttingPlanId: item.cuttingPlanId,
    material: item.materialName ?? '—',
    efficiencyPercent: item.efficiencyPercent,
    status: item.isComplete ? 'optimized' : 'inProgress',
    lastPlanAtUtc: item.lastPlanAtUtc,
  };
}
