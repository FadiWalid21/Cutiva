import { DashboardSnapshot } from './dashboard.models';

/** Offline fallback when `environment.useDashboardMock` is true. */
export const DASHBOARD_MOCK: DashboardSnapshot = {
  kpis: {
    materialEfficiencyPercent: 94.8,
    wastePercent: 5.2,
    stockBarsUsed: 126,
    totalMaterialCost: 8420,
  },
  remnantInventory: [
    {
      id: 'rem-1',
      materialId: 'mat-1',
      materialName: 'Aluminum 6063-T6',
      lengthMm: 2070,
      quantity: 1,
    },
    {
      id: 'rem-2',
      materialId: 'mat-2',
      materialName: 'Aluminum Profile 40×40',
      lengthMm: 4800,
      quantity: 2,
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
