import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { CuttingPlan, CuttingPlansApi } from '../../../core/api/cutting-plans.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';

@Component({
  selector: 'app-plan-detail',
  imports: [TranslatePipe, RouterLink, DecimalPipe, DatePipe],
  templateUrl: './plan-detail.html',
  styleUrl: './plan-detail.scss',
})
export class PlanDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly plansApi = inject(CuttingPlansApi);

  readonly plan = signal<CuttingPlan | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly approving = signal(false);
  readonly notice = signal<string | null>(null);

  readonly isI18nKey = isI18nKey;

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) this.load(id);
    });
  }

  load(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.plansApi.getById(id).subscribe({
      next: (plan) => {
        this.plan.set(plan);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(apiErrorMessage(err, 'plans.notFound'));
        this.loading.set(false);
      },
    });
  }

  isDraft(): boolean {
    const status = this.plan()?.status ?? 'Draft';
    return status === 'Draft';
  }

  approve(): void {
    const plan = this.plan();
    if (!plan) return;

    this.approving.set(true);
    this.notice.set(null);
    this.error.set(null);
    this.plansApi.approve(plan.cuttingPlanId).subscribe({
      next: (updated) => {
        this.plan.set(updated);
        this.approving.set(false);
        this.notice.set('plans.approveSuccess');
      },
      error: (err) => {
        this.approving.set(false);
        this.error.set(apiErrorMessage(err, 'plans.approveError'));
      },
    });
  }
}
