import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { FeatureFlagsService } from '../services/feature-flags.service';

export const resourcesV2Match: CanMatchFn = () =>
  inject(FeatureFlagsService).resourcesV2;

export const productTemplatesMatch: CanMatchFn = () =>
  inject(FeatureFlagsService).productTemplates;

/**
 * /materials: show legacy page when V2 is off; redirect to /resources when on.
 * (Cannot combine redirectTo + canMatch — NG04014.)
 */
export const materialsRouteActivate: CanActivateFn = () => {
  const features = inject(FeatureFlagsService);
  const router = inject(Router);
  if (features.resourcesV2) {
    return router.createUrlTree(['/resources']);
  }
  return true;
};

/** Resources / product recipes: Owner or Admin only. */
export const catalogManageActivate: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.canManageCatalog()) {
    return true;
  }
  return router.createUrlTree(['/projects']);
};
