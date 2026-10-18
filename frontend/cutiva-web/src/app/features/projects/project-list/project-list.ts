import { Component, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ProjectListItem, ProjectsApi } from '../../../core/api/projects.api';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';

@Component({
  selector: 'app-project-list',
  imports: [FormsModule, RouterLink, TranslatePipe],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss',
})
export class ProjectList {
  private readonly projectsApi = inject(ProjectsApi);
  readonly workshopContext = inject(WorkshopContextService);

  readonly projects = signal<ProjectListItem[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly editingId = signal<string | null>(null);
  editName = '';
  editCustomer = '';

  constructor() {
    effect(() => {
      const workshopId = this.workshopContext.selectedWorkshopId();
      const loaded = this.workshopContext.loaded();
      if (!loaded) return;
      if (!workshopId) {
        this.projects.set([]);
        return;
      }
      this.load(workshopId);
    });
  }

  load(workshopId: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.projectsApi.list(workshopId, 1, 100).subscribe({
      next: (page) => {
        this.projects.set(page.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('common.loadError');
        this.loading.set(false);
      },
    });
  }

  startEdit(project: ProjectListItem): void {
    this.editingId.set(project.id);
    this.editName = project.name;
    this.editCustomer = project.customerName ?? '';
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(project: ProjectListItem): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId || !this.editName.trim()) return;

    this.projectsApi
      .update(workshopId, project.id, {
        name: this.editName.trim(),
        customerName: this.editCustomer.trim() || null,
      })
      .subscribe({
        next: () => {
          this.editingId.set(null);
          this.load(workshopId);
        },
        error: () => this.error.set('common.saveError'),
      });
  }

  remove(project: ProjectListItem): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) return;

    this.projectsApi.delete(workshopId, project.id).subscribe({
      next: () => this.load(workshopId),
      error: () => this.error.set('common.deleteError'),
    });
  }
}
