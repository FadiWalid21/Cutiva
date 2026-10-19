import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { AdminApi, AdminPlan } from '../../../core/api/admin.api';

interface PlanDraft {
  id: string;
  code: string;
  name: string;
  priceSar: number;
  durationDays: number;
  maxUsers: number | null;
  unlimited: boolean;
  isActive: boolean;
}

@Component({
  selector: 'app-admin-plans',
  imports: [TranslatePipe, FormsModule],
  templateUrl: './admin-plans.html',
  styleUrl: './admin-plans.scss',
})
export class AdminPlans implements OnInit {
  private readonly adminApi = inject(AdminApi);
  readonly drafts = signal<PlanDraft[]>([]);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);
  readonly savingId = signal<string | null>(null);

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.adminApi.listPlans().subscribe({
      next: (plans) => this.drafts.set(plans.map((p) => this.toDraft(p))),
      error: () => this.error.set('common.loadError'),
    });
  }

  save(draft: PlanDraft): void {
    this.error.set(null);
    this.notice.set(null);
    this.savingId.set(draft.id);
    this.adminApi
      .updatePlan(draft.id, {
        name: draft.name.trim(),
        priceSar: Number(draft.priceSar),
        maxUsers: draft.unlimited ? null : Number(draft.maxUsers) || 1,
        durationDays: Number(draft.durationDays) || 30,
        isActive: draft.isActive,
      })
      .subscribe({
        next: (plan) => {
          this.savingId.set(null);
          this.drafts.update((list) =>
            list.map((d) => (d.id === plan.id ? this.toDraft(plan) : d)),
          );
          this.notice.set('admin.planSaved');
        },
        error: () => {
          this.savingId.set(null);
          this.error.set('common.saveError');
        },
      });
  }

  private toDraft(p: AdminPlan): PlanDraft {
    return {
      id: p.id,
      code: p.code,
      name: p.name,
      priceSar: p.priceSar,
      durationDays: p.durationDays,
      maxUsers: p.maxUsers,
      unlimited: p.maxUsers == null,
      isActive: p.isActive,
    };
  }
}
