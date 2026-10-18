import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MaterialsApi, Material } from '../../../core/api/materials.api';
import { ProjectsApi } from '../../../core/api/projects.api';
import {
  ProjectSection,
  ProjectSectionsApi,
} from '../../../core/api/project-sections.api';
import {
  RequiredCut,
  RequiredCutsApi,
} from '../../../core/api/required-cuts.api';
import { Sector, SectorsApi } from '../../../core/api/sectors.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';

@Component({
  selector: 'app-project-workspace',
  imports: [FormsModule, RouterLink, TranslatePipe],
  templateUrl: './project-workspace.html',
  styleUrl: './project-workspace.scss',
})
export class ProjectWorkspace {
  private readonly route = inject(ActivatedRoute);
  private readonly sectionsApi = inject(ProjectSectionsApi);
  private readonly cutsApi = inject(RequiredCutsApi);
  private readonly sectorsApi = inject(SectorsApi);
  private readonly materialsApi = inject(MaterialsApi);
  private readonly projectsApi = inject(ProjectsApi);
  private readonly workshopContext = inject(WorkshopContextService);

  readonly projectId = signal<string>('');
  readonly projectName = signal<string>('');
  readonly sections = signal<ProjectSection[]>([]);
  readonly cuts = signal<RequiredCut[]>([]);
  readonly sectors = signal<Sector[]>([]);
  readonly materials = signal<Material[]>([]);
  readonly selectedSectionId = signal<string | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly isI18nKey = isI18nKey;

  sectionName = '';
  sectionSectorId = '';
  cutName = '';
  cutMaterialId = '';
  cutLengthMm = 1000;
  cutQuantity = 1;

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('projectId') ?? '';
      this.projectId.set(id);
      if (id) this.bootstrap(id);
    });
  }

  private bootstrap(projectId: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.resolveProjectName(projectId);

    this.sectorsApi.list().subscribe({
      next: (sectors) => {
        this.sectors.set(sectors);
        if (!this.sectionSectorId && sectors.length) {
          this.sectionSectorId = sectors[0].id;
        }
      },
    });
    this.materialsApi.list().subscribe({
      next: (materials) => {
        this.materials.set(materials);
        if (!this.cutMaterialId && materials.length) {
          this.cutMaterialId = materials[0].id;
        }
      },
    });
    this.reloadSections(projectId);
  }

  private resolveProjectName(projectId: string): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.projectName.set('');
      return;
    }

    this.projectsApi.list(workshopId, 1, 100).subscribe({
      next: (page) => {
        const match = page.items.find((p) => p.id === projectId);
        this.projectName.set(match?.name ?? '');
      },
      error: () => this.projectName.set(''),
    });
  }

  reloadSections(projectId = this.projectId()): void {
    this.sectionsApi.list(projectId).subscribe({
      next: (sections) => {
        this.sections.set(sections);
        this.loading.set(false);
        const selected =
          this.selectedSectionId() &&
          sections.some((s) => s.id === this.selectedSectionId())
            ? this.selectedSectionId()!
            : (sections[0]?.id ?? null);
        this.selectedSectionId.set(selected);
        if (selected) this.loadCuts(selected);
        else this.cuts.set([]);
      },
      error: (err) => {
        this.error.set(apiErrorMessage(err, 'common.loadError'));
        this.loading.set(false);
      },
    });
  }

  selectSection(id: string): void {
    this.selectedSectionId.set(id);
    this.loadCuts(id);
  }

  loadCuts(sectionId: string): void {
    this.cutsApi.list({ projectSectionId: sectionId }).subscribe({
      next: (cuts) => this.cuts.set(cuts),
      error: (err) => this.error.set(apiErrorMessage(err, 'common.loadError')),
    });
  }

  addSection(): void {
    if (!this.sectionName.trim() || !this.sectionSectorId) return;
    this.sectionsApi
      .create(this.projectId(), {
        name: this.sectionName.trim(),
        sectorId: this.sectionSectorId,
      })
      .subscribe({
        next: () => {
          this.sectionName = '';
          this.reloadSections();
        },
        error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
      });
  }

  addCut(): void {
    const sectionId = this.selectedSectionId();
    if (!sectionId || !this.cutName.trim() || !this.cutMaterialId) return;

    this.cutsApi
      .create({
        projectSectionId: sectionId,
        materialId: this.cutMaterialId,
        name: this.cutName.trim(),
        lengthMm: Number(this.cutLengthMm),
        quantity: Number(this.cutQuantity),
      })
      .subscribe({
        next: () => {
          this.cutName = '';
          this.loadCuts(sectionId);
        },
        error: (err) => this.error.set(apiErrorMessage(err, 'common.saveError')),
      });
  }

  removeCut(cut: RequiredCut): void {
    this.cutsApi.delete(cut.id).subscribe({
      next: () => {
        const sectionId = this.selectedSectionId();
        if (sectionId) this.loadCuts(sectionId);
      },
      error: (err) => this.error.set(apiErrorMessage(err, 'common.deleteError')),
    });
  }
}
