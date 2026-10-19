import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import {
  CuttingPlan,
  CuttingPlanBar,
  CuttingPlansApi,
} from '../../../core/api/cutting-plans.api';
import {
  ProjectSection,
  ProjectSectionsApi,
} from '../../../core/api/project-sections.api';
import { QuotesApi } from '../../../core/api/quotes.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { formatCm } from '../../../core/units/length';
import { AluminumFinishBar } from '../../../shared/aluminum-finish-bar/aluminum-finish-bar';
import { FramePreview } from '../../../shared/frame-preview/frame-preview';

export interface BuyLine {
  lengthMm: number;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  isRemnant: boolean;
}

export interface UnallocatedGroup {
  name: string;
  lengthMm: number;
  quantity: number;
}

@Component({
  selector: 'app-plan-detail',
  imports: [
    TranslatePipe,
    RouterLink,
    DecimalPipe,
    DatePipe,
    FramePreview,
    AluminumFinishBar,
  ],
  templateUrl: './plan-detail.html',
  styleUrl: './plan-detail.scss',
})
export class PlanDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly plansApi = inject(CuttingPlansApi);
  private readonly quotesApi = inject(QuotesApi);
  private readonly sectionsApi = inject(ProjectSectionsApi);

  readonly plan = signal<CuttingPlan | null>(null);
  readonly sections = signal<ProjectSection[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly approving = signal(false);
  readonly openingQuote = signal(false);
  readonly notice = signal<string | null>(null);

  readonly isI18nKey = isI18nKey;
  readonly formatCm = formatCm;

  readonly previewSections = computed(() =>
    this.sections().filter(
      (s) => s.heightMm && s.heightMm > 0 && s.widthMm && s.widthMm > 0,
    ),
  );

  readonly buyList = computed(() => this.buildBuyList(this.plan()?.bars ?? []));

  /** Market purchases only (!isRemnant && unitPrice > 0). */
  readonly marketBuyList = computed(() =>
    this.buyList().filter((line) => !line.isRemnant),
  );

  /** Remnant warehouse rows used in this plan (price 0). */
  readonly remnantBuyList = computed(() =>
    this.buyList().filter((line) => line.isRemnant),
  );

  readonly marketBarsCount = computed(() =>
    this.marketBuyList().reduce((sum, line) => sum + line.quantity, 0),
  );

  readonly remnantBarsCount = computed(() =>
    this.remnantBuyList().reduce((sum, line) => sum + line.quantity, 0),
  );

  readonly unallocatedGroups = computed(() => {
    const pieces = this.plan()?.unallocatedPieces ?? [];
    const map = new Map<string, UnallocatedGroup>();
    for (const piece of pieces) {
      const key = `${piece.name}|${piece.lengthMm}`;
      const existing = map.get(key);
      if (existing) {
        existing.quantity += 1;
      } else {
        map.set(key, {
          name: piece.name,
          lengthMm: piece.lengthMm,
          quantity: 1,
        });
      }
    }
    return [...map.values()].sort((a, b) => b.lengthMm - a.lengthMm);
  });

  readonly unallocatedCount = computed(
    () => this.plan()?.unallocatedPieces.length ?? 0,
  );

  readonly wastePercent = computed(() => {
    const p = this.plan();
    if (!p || p.totalStockLengthMm <= 0) return 0;
    return (p.totalLeftoverMm / p.totalStockLengthMm) * 100;
  });

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) this.load(id);
    });
  }

  load(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.sections.set([]);
    this.plansApi.getById(id).subscribe({
      next: (plan) => {
        this.plan.set(plan);
        this.loading.set(false);
        if (plan.projectId) {
          this.sectionsApi.list(plan.projectId).subscribe({
            next: (sections) => this.sections.set(sections),
          });
        }
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

  isComplete(): boolean {
    return this.plan()?.isComplete === true;
  }

  statusLabelKey(): string {
    return this.isDraft() ? 'plans.draft' : 'plans.approved';
  }

  approve(): void {
    const plan = this.plan();
    if (!plan || !plan.isComplete) return;

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

  openQuote(): void {
    const plan = this.plan();
    if (!plan?.projectId || this.isDraft()) return;

    this.openingQuote.set(true);
    this.error.set(null);

    const go = (quoteId: string) => {
      this.openingQuote.set(false);
      void this.router.navigate(['/quotes', quoteId]);
    };

    this.quotesApi.getByPlan(plan.projectId, plan.cuttingPlanId).subscribe({
      next: (quote) => go(quote.id),
      error: () => {
        this.quotesApi
          .upsert({
            projectId: plan.projectId!,
            cuttingPlanId: plan.cuttingPlanId,
            surcharges: [],
          })
          .subscribe({
            next: (quote) => go(quote.id),
            error: (err) => {
              this.openingQuote.set(false);
              this.error.set(apiErrorMessage(err, 'quote.loadError'));
            },
          });
      },
    });
  }

  private buildBuyList(bars: CuttingPlanBar[]): BuyLine[] {
    const map = new Map<string, BuyLine>();
    for (const bar of bars) {
      // Remnant bars are stored at price 0; never treat them as market SKUs.
      const isRemnant = !!bar.isRemnant || bar.unitPrice <= 0;
      const unitPrice = isRemnant ? 0 : bar.unitPrice;
      if (!isRemnant && unitPrice <= 0) continue;
      const key = `${bar.lengthMm}|${unitPrice}|${isRemnant ? 'R' : 'M'}`;
      const existing = map.get(key);
      if (existing) {
        existing.quantity += 1;
        existing.lineTotal += unitPrice;
      } else {
        map.set(key, {
          lengthMm: bar.lengthMm,
          quantity: 1,
          unitPrice,
          lineTotal: unitPrice,
          isRemnant,
        });
      }
    }
    return [...map.values()].sort((a, b) => b.lengthMm - a.lengthMm);
  }
}
