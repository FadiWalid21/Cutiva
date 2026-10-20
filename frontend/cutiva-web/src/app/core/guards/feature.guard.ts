import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { PlanEntitlementsService } from '../services/plan-entitlements.service';
import type { PlanFeatureCode } from '../constants/plan-features';

/** Resources / product recipes: Owner or Admin only. */
export const catalogManageActivate: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.canManageCatalog()) {
    return true;
  }
  return router.createUrlTree(['/projects']);
};

/** Gate a route by the workshop's current plan entitlements. */
export function planFeatureActivate(feature: PlanFeatureCode): CanActivateFn {
  return () => {
    const entitlements = inject(PlanEntitlementsService);
    const router = inject(Router);
    if (entitlements.has(feature)) {
      return true;
    }
    return router.createUrlTree(['/dashboard']);
  };
}
