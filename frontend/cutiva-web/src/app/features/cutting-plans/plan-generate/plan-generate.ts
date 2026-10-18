import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { forkJoin } from 'rxjs';
import { CuttingPlansApi } from '../../../core/api/cutting-plans.api';
import { MaterialsApi, Material } from '../../../core/api/materials.api';
import { ProjectsApi, ProjectListItem } from '../../../core/api/projects.api';
import { ProjectSectionsApi } from '../../../core/api/project-sections.api';
import { RequiredCut, RequiredCutsApi } from '../../../core/api/required-cuts.api';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';

@Component({
  selector: 'app-plan-generate',
  imports: [FormsModule, RouterLink, TranslatePipe],
  templateUrl: './plan-generate.html',
  styleUrl: './plan-generate.scss',
})
export class PlanGenerate {
  private readonly projectsApi = inject(ProjectsApi);
  private readonly sectionsApi = inject(ProjectSectionsApi);
  private readonly cutsApi = inject(RequiredCutsApi);
  private readonly materialsApi = inject(MaterialsApi);
  private readonly plansApi = inject(CuttingPlansApi);
  private readonly router = inject(Router);
  readonly workshopContext = inject(WorkshopContextService);

  readonly materials = signal<Material[]>([]);
  readonly projects = signal<ProjectListItem[]>([]);
  readonly cuts = signal<RequiredCut[]>([]);
  readonly selectedCutIds = signal<Set<string>>(new Set());
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  materialId = '';
  kerfMm = 3;
  projectId = '';

  constructor() {
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

  onProjectOrMaterialChange(): void {
    this.reloadCuts();
  }

  private reloadCuts(): void {
    if (!this.projectId || !this.materialId) {
      this.cuts.set([]);
      this.selectedCutIds.set(new Set());
      return;
    }

    this.sectionsApi.list(this.projectId).subscribe({
      next: (sections) => {
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

  toggleCut(id: string, checked: boolean): void {
    const next = new Set(this.selectedCutIds());
    if (checked) next.add(id);
    else next.delete(id);
    this.selectedCutIds.set(next);
  }

  isSelected(id: string): boolean {
    return this.selectedCutIds().has(id);
  }

  generate(): void {
    const ids = [...this.selectedCutIds()];
    if (!this.materialId || ids.length === 0) {
      this.error.set('plans.selectCuts');
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.plansApi
      .generate({
        materialId: this.materialId,
        kerfMm: Number(this.kerfMm),
        requiredCutIds: ids,
      })
      .subscribe({
        next: (plan) => {
          this.saving.set(false);
          void this.router.navigate(['/cutting-plans', plan.cuttingPlanId]);
        },
        error: () => {
          this.saving.set(false);
          this.error.set('plans.generateError');
        },
      });
  }
}
