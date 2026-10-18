import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import {
  apiErrorMessage,
  apiFieldErrors,
  firstFieldError,
  isI18nKey,
} from '../../../core/http/api-error';
import { ProjectsApi } from '../../../core/api/projects.api';
import { MotionService } from '../../../core/motion/motion.service';
import { WorkshopContextService } from '../../../core/services/workshop-context.service';
import { FormFieldError } from '../../../shared/form-field/form-field-error';

@Component({
  selector: 'app-project-create',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, FormFieldError],
  templateUrl: './project-create.html',
  styleUrl: './project-create.scss',
})
export class ProjectCreate implements AfterViewInit {
  private readonly projectsApi = inject(ProjectsApi);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly motion = inject(MotionService);
  readonly workshopContext = inject(WorkshopContextService);

  private readonly root = viewChild<ElementRef<HTMLElement>>('pageRoot');

  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly fieldErrors = signal<{
    name: string | null;
    customerName: string | null;
  }>({
    name: null,
    customerName: null,
  });
  readonly isI18nKey = isI18nKey;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    customerName: ['', [Validators.maxLength(150)]],
  });

  ngAfterViewInit(): void {
    this.motion.pageEnter(this.root()?.nativeElement ?? null);
  }

  submit(): void {
    const workshopId = this.workshopContext.selectedWorkshopId();
    if (!workshopId) {
      this.error.set('dashboard.noWorkshop');
      return;
    }

    this.form.markAllAsTouched();
    this.error.set(null);
    this.fieldErrors.set({ name: null, customerName: null });

    if (this.form.invalid) {
      const nameCtrl = this.form.controls.name;
      this.fieldErrors.set({
        name: nameCtrl.errors?.['required']
          ? 'projects.validation.nameRequired'
          : nameCtrl.errors?.['maxlength']
            ? 'projects.validation.nameMax'
            : null,
        customerName: this.form.controls.customerName.errors?.['maxlength']
          ? 'projects.validation.customerMax'
          : null,
      });
      return;
    }

    const { name, customerName } = this.form.getRawValue();
    this.saving.set(true);

    this.projectsApi
      .create(workshopId, {
        name: name.trim(),
        customerName: customerName.trim() || null,
      })
      .subscribe({
        next: (created) => {
          this.saving.set(false);
          void this.router.navigate(['/projects', created.id]);
        },
        error: (err) => {
          this.saving.set(false);
          const fields = apiFieldErrors(err);
          this.fieldErrors.set({
            name: firstFieldError(fields, 'name'),
            customerName: firstFieldError(fields, 'customerName'),
          });
          this.error.set(apiErrorMessage(err, 'common.saveError'));
        },
      });
  }
}
