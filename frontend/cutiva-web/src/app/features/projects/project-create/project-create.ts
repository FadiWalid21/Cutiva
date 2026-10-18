import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ProjectsApi } from '../../../core/api/projects.api';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';

@Component({
  selector: 'app-project-create',
  imports: [FormsModule, RouterLink, TranslatePipe],
  templateUrl: './project-create.html',
  styleUrl: './project-create.scss',
})
export class ProjectCreate {
  private readonly projectsApi = inject(ProjectsApi);
  private readonly router = inject(Router);
  readonly workshopContext = inject(WorkshopContextService);

  name = '';
  customerName = '';
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  submit(): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.error.set('dashboard.noWorkshop');
      return;
    }
    if (!this.name.trim()) return;

    this.saving.set(true);
    this.error.set(null);
    this.projectsApi
      .create(workshopId, {
        name: this.name.trim(),
        customerName: this.customerName.trim() || null,
      })
      .subscribe({
        next: (created) => {
          this.saving.set(false);
          void this.router.navigate(['/projects', created.id]);
        },
        error: () => {
          this.saving.set(false);
          this.error.set('common.saveError');
        },
      });
  }
}
