import { DashboardSnapshot } from './dashboard.models';

/** Offline fallback when `environment.useDashboardMock` is true. */
export const DASHBOARD_MOCK: DashboardSnapshot = {
  kpis: {
    materialEfficiencyPercent: 94.8,
    wastePercent: 5.2,
    stockBarsUsed: 126,
    totalMaterialCost: 8420,
  },
  projectedLeftovers: [
    {
      id: 'rem-1',
      cuttingPlanId: 'plan-1',
      lengthMm: 207,
      percentOfBar: 3.4,
      sourceBarLabel: '6000 mm',
      materialName: 'Aluminum 6063-T6',
      isProjected: true,
    },
    {
      id: 'rem-2',
      cuttingPlanId: 'plan-2',
      lengthMm: 480,
      percentOfBar: 8,
      sourceBarLabel: '6000 mm',
      materialName: 'Aluminum Profile 40×40',
      isProjected: true,
    },
  ],
  recentProjects: [
    {
      id: 'p1',
      name: 'Villa Windows Phase 2',
      cuttingPlanId: 'plan-1',
      material: 'Aluminum 6063-T6',
      efficiencyPercent: 96.2,
      status: 'optimized',
      lastPlanAtUtc: new Date().toISOString(),
    },
    {
      id: 'p2',
      name: 'Office Facade Frames',
      cuttingPlanId: 'plan-2',
      material: 'Aluminum Profiles',
      efficiencyPercent: 91.4,
      status: 'inProgress',
      lastPlanAtUtc: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
    },
  ],
};
