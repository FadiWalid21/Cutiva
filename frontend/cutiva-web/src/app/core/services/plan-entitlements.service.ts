import { Injectable, computed, inject } from '@angular/core';
import { AuthService } from './auth.service';
import {
  PlanFeatureCode,
  PlanFeatureCodes,
} from '../constants/plan-features';
import { environment } from '../../../environments/environment';

/**
 * Plan entitlements from auth/me. Env flags remain hard kill-switches when false.
 */
@Injectable({ providedIn: 'root' })
export class PlanEntitlementsService {
  private readonly auth = inject(AuthService);

  readonly features = computed(() => {
    const fromMe = this.auth.currentUser()?.features ?? [];
    return new Set(fromMe);
  });

  has(code: PlanFeatureCode): boolean {
    if (code === PlanFeatureCodes.resources && environment.features?.resourcesV2 === false) {
      return false;
    }
    if (
      code === PlanFeatureCodes.productTemplates &&
      environment.features?.productTemplates === false
    ) {
      return false;
    }
    return this.features().has(code);
  }

  readonly resources = computed(() => this.has(PlanFeatureCodes.resources));
  readonly productTemplates = computed(() =>
    this.has(PlanFeatureCodes.productTemplates),
  );
  readonly remnants = computed(() => this.has(PlanFeatureCodes.remnants));
  readonly planComparisons = computed(() =>
    this.has(PlanFeatureCodes.planComparisons),
  );
  readonly clientQuotes = computed(() => this.has(PlanFeatureCodes.clientQuotes));
  readonly workshopReports = computed(() =>
    this.has(PlanFeatureCodes.workshopReports),
  );
}
