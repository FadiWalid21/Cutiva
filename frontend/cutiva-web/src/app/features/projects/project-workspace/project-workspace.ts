import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MaterialsApi, Material } from '../../../core/api/materials.api';
import {
  ProjectSection,
  ProjectSectionsApi,
} from '../../../core/api/project-sections.api';
import {
  RequiredCut,
  RequiredCutsApi,
} from '../../../core/api/required-cuts.api';
import { Sector, SectorsApi } from '../../../core/api/sectors.api';

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

  readonly projectId = signal<string>('');
  readonly sections = signal<ProjectSection[]>([]);
  readonly cuts = signal<RequiredCut[]>([]);
  readonly sectors = signal<Sector[]>([]);
  readonly materials = signal<Material[]>([]);
  readonly selectedSectionId = signal<string | null>(null);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  sectionName = '';
  sectionSectorId = '';
  cutName = '';
  cutMaterialId = '';
  cutLengthMm = 1000;
  cutQuantity = 1;

  constructor() {
    effect(() => {
      const id = this.route.snapshot.paramMap.get('projectId') ?? '';
      this.projectId.set(id);
      if (id) {
        this.bootstrap(id);
      }
    });

    this.route.paramMap.subscribe((params) => {
      const id = params.get('projectId') ?? '';
      this.projectId.set(id);
      if (id) this.bootstrap(id);
    });
  }

  private bootstrap(projectId: string): void {
    this.loading.set(true);
    this.error.set(null);
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
      error: () => {
        this.error.set('common.loadError');
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
      error: () => this.error.set('common.loadError'),
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
        error: () => this.error.set('common.saveError'),
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
        error: () => this.error.set('common.saveError'),
      });
  }

  removeCut(cut: RequiredCut): void {
    this.cutsApi.delete(cut.id).subscribe({
      next: () => {
        const sectionId = this.selectedSectionId();
        if (sectionId) this.loadCuts(sectionId);
      },
      error: () => this.error.set('common.deleteError'),
    });
  }
}
