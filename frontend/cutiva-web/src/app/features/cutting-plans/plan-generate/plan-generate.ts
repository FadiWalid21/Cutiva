import { DecimalPipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import {
  CuttingPlanAlternativeSummary,
  CuttingPlanComparison,
  CuttingPlansApi,
} from '../../../core/api/cutting-plans.api';
import { MaterialsApi, Material } from '../../../core/api/materials.api';
import { ProjectsApi, ProjectListItem } from '../../../core/api/projects.api';
import {
  ProjectSection,
  ProjectSectionsApi,
} from '../../../core/api/project-sections.api';
import { RequiredCut, RequiredCutsApi } from '../../../core/api/required-cuts.api';
import { StockBar, StockBarsApi } from '../../../core/api/stock-bars.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';
import { cmToMm, formatCm } from '../../../core/units/length';
import { AluminumFinishBar } from '../../../shared/aluminum-finish-bar/aluminum-finish-bar';
import { FramePreview } from '../../../shared/frame-preview/frame-preview';

@Component({
  selector: 'app-plan-generate',
  imports: [
    FormsModule,
    RouterLink,
    TranslatePipe,
    DecimalPipe,
    FramePreview,
    AluminumFinishBar,
  ],
  templateUrl: './plan-generate.html',
  styleUrl: './plan-generate.scss',
})
export class PlanGenerate {
  private readonly projectsApi = inject(ProjectsApi);
  private readonly sectionsApi = inject(ProjectSectionsApi);
  private readonly cutsApi = inject(RequiredCutsApi);
  private readonly materialsApi = inject(MaterialsApi);
  private readonly plansApi = inject(CuttingPlansApi);
  private readonly stockBarsApi = inject(StockBarsApi);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly workshopContext = inject(WorkshopContextService);

  readonly materials = signal<Material[]>([]);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly sections = signal<ProjectSection[]>([]);
  readonly cuts = signal<RequiredCut[]>([]);
  readonly selectedCutIds = signal<Set<string>>(new Set());
  readonly comparison = signal<CuttingPlanComparison | null>(null);
  readonly remnantBars = signal<StockBar[]>([]);

  readonly previewSections = computed(() =>
    this.sections().filter(
      (s) => s.heightMm && s.heightMm > 0 && s.widthMm && s.widthMm > 0,
    ),
  );

  /** Remnants long enough for the longest selected cut (matches FFD open-bar eligibility). */
  readonly usableRemnantCount = computed(() => {
    const remnants = this.remnantBars().filter(
      (b) => b.isActive && b.availableQuantity > 0,
    );
    const selected = this.cuts().filter((c) => this.selectedCutIds().has(c.id));
    if (selected.length === 0) {
      return remnants.reduce((sum, b) => sum + b.availableQuantity, 0);
    }
    const maxLength = Math.max(...selected.map((c) => c.lengthMm));
    return remnants
      .filter((b) => b.lengthMm >= maxLength)
      .reduce((sum, b) => sum + b.availableQuantity, 0);
  });

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  /** Cleared results after UseRemnants toggle — user must regenerate. */
  readonly needsRegenerate = signal(false);

  readonly isI18nKey = isI18nKey;
  readonly formatCm = formatCm;

  materialId = '';
  kerfCm = 0.3;
  projectId = '';
  useRemnants = true;

  constructor() {
    const qp = this.route.snapshot.queryParamMap;
    const projectQp = qp.get('projectId');
    if (projectQp) this.projectId = projectQp;

    const groupId = qp.get('groupId');
    if (groupId) this.loadComparison(groupId);

    effect(() => {
      const workshopId = this.workshopContext.selectedWorkshopId();
      if (!workshopId || !this.workshopContext.loaded()) return;
      this.loadContext(workshopId);
    });
  }

  private loadContext(workshopId: string): void {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      materials: this.materialsApi.list(),
      projects: this.projectsApi.list(workshopId, 1, 100),
    }).subscribe({
      next: ({ materials, projects }) => {
        this.materials.set(materials);
        this.projects.set(projects.items);
        if (!this.materialId && materials.length) {
          this.materialId = materials[0].id;
        } else if (
          this.materialId &&
          !materials.some((m) => m.id === this.materialId)
        ) {
          this.materialId = materials[0]?.id ?? '';
        }
        if (!this.projectId && projects.items.length) {
          this.projectId = projects.items[0].id;
        }
        this.loading.set(false);
        this.reloadCuts();
      },
      error: () => {
        this.error.set('common.loadError');
        this.loading.set(false);
      },
    });
  }

  private loadComparison(groupId: string): void {
    this.plansApi.getComparison(groupId).subscribe({
      next: (comparison) => this.comparison.set(comparison),
      error: () => this.error.set('plans.comparisonLoadError'),
    });
  }

  onProjectOrMaterialChange(): void {
    this.comparison.set(null);
    this.reloadCuts();
    this.reloadRemnants();
  }

  onUseRemnantsChange(): void {
    this.comparison.set(null);
    this.needsRegenerate.set(true);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { groupId: null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  private reloadCuts(): void {
    if (!this.projectId || !this.materialId) {
      this.sections.set([]);
      this.cuts.set([]);
      this.selectedCutIds.set(new Set());
      this.remnantBars.set([]);
      return;
    }

    this.reloadRemnants();

    this.sectionsApi.list(this.projectId).subscribe({
      next: (sections) => {
        this.sections.set(sections);
        if (sections.length === 0) {
          this.cuts.set([]);
          this.selectedCutIds.set(new Set());
          return;
        }

        forkJoin(
          sections.map((section) =>
            this.cutsApi.list({
              projectSectionId: section.id,
              materialId: this.materialId,
            }),
          ),
        ).subscribe({
          next: (groups) => {
            const all = groups.flat();
            this.cuts.set(all);
            this.selectedCutIds.set(new Set(all.map((c) => c.id)));
          },
          error: () => this.error.set('common.loadError'),
        });
      },
      error: () => this.error.set('common.loadError'),
    });
  }

  private reloadRemnants(): void {
    if (!this.materialId) {
      this.remnantBars.set([]);
      return;
    }

    this.stockBarsApi
      .list({
        materialId: this.materialId,
        isActive: true,
        isRemnant: true,
      })
      .subscribe({
        next: (bars) => this.remnantBars.set(bars),
        error: () => this.remnantBars.set([]),
      });
  }

  toggleCut(id: string, checked: boolean): void {
    const next = new Set(this.selectedCutIds());
    if (checked) next.add(id);
    else next.delete(id);
    this.selectedCutIds.set(next);
  }

  isSelected(id: string): boolean {
    return this.selectedCutIds().has(id);
  }

  strategyLabelKey(code: string): string {
    if (code === 'MixedCheapestUnit') return 'plans.strategy.mixedCheapest';
    if (code === 'MixedLeastWasteOpen') return 'plans.strategy.mixedWaste';
    if (code.startsWith('SingleSku_')) return 'plans.strategy.singleSku';
    return 'plans.strategy.other';
  }

  strategyLengthCm(code: string): string {
    if (!code.startsWith('SingleSku_')) return '';
    const mm = Number(code.slice('SingleSku_'.length));
    if (!Number.isFinite(mm)) return '';
    return formatCm(mm);
  }

  isBestCost(alt: CuttingPlanAlternativeSummary): boolean {
    const c = this.comparison();
    return !!c && c.recommendedCostPlanId === alt.cuttingPlanId;
  }

  isBestWaste(alt: CuttingPlanAlternativeSummary): boolean {
    const c = this.comparison();
    return !!c && c.recommendedWastePlanId === alt.cuttingPlanId;
  }

  openPlan(id: string): void {
    void this.router.navigate(['/cutting-plans', id]);
  }

  generate(): void {
    const ids = [...this.selectedCutIds()];
    if (!this.materialId || ids.length === 0) {
      this.error.set('plans.selectCuts');
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.needsRegenerate.set(false);
    this.comparison.set(null);
    this.plansApi
      .generate({
        materialId: this.materialId,
        kerfMm: cmToMm(Number(this.kerfCm)),
        requiredCutIds: ids,
        useRemnants: !!this.useRemnants,
      })
      .subscribe({
        next: (comparison) => {
          this.saving.set(false);
          this.needsRegenerate.set(false);
          this.comparison.set(comparison);
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: {
              projectId: this.projectId || null,
              groupId: comparison.comparisonGroupId,
            },
            queryParamsHandling: 'merge',
            replaceUrl: true,
          });
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'plans.generateError'));
        },
      });
  }
}
