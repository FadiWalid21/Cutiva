import { Component, OnInit, ElementRef, inject, signal, viewChild } from '@angular/core';
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
import { MotionService } from '../../../core/motion/motion.service';
import { AuthService } from '../../../core/services/auth.service';
import { FormFieldError } from '../../../shared/form-field/form-field-error';

@Component({
  selector: 'app-signup',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, FormFieldError],
  templateUrl: './signup.html',
  styleUrl: './signup.scss',
})
export class Signup implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly motion = inject(MotionService);

  private readonly brand = viewChild<ElementRef<HTMLElement>>('brand');
  private readonly panel = viewChild<ElementRef<HTMLElement>>('panel');

  readonly submitting = signal(false);
  readonly formError = signal<string | null>(null);
  readonly fieldErrors = signal<{
    email: string | null;
    password: string | null;
    displayName: string | null;
  }>({
    email: null,
    password: null,
    displayName: null,
  });

  readonly isI18nKey = isI18nKey;

  readonly form = this.fb.nonNullable.group({
    displayName: ['', [Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  ngOnInit(): void {
    queueMicrotask(() => {
      this.motion.authPanelEnter(
        this.panel()?.nativeElement ?? null,
        this.brand()?.nativeElement ?? null,
      );
    });
  }

  submit(): void {
    this.form.markAllAsTouched();
    this.formError.set(null);
    this.fieldErrors.set({ email: null, password: null, displayName: null });

    if (this.form.invalid) {
      this.fieldErrors.set({
        email: this.clientError('email'),
        password: this.clientError('password'),
        displayName: this.clientError('displayName'),
      });
      return;
    }

    const { email, password, displayName } = this.form.getRawValue();
    this.submitting.set(true);

    this.auth.register(email.trim(), password, displayName.trim() || null).subscribe({
      next: () => {
        this.submitting.set(false);
        void this.router.navigateByUrl('/');
      },
      error: (err) => {
        this.submitting.set(false);
        const fields = apiFieldErrors(err);
        this.fieldErrors.set({
          email: firstFieldError(fields, 'email'),
          password: firstFieldError(fields, 'password'),
          displayName: firstFieldError(fields, 'displayName'),
        });
        this.formError.set(apiErrorMessage(err, 'auth.errors.generic'));
      },
    });
  }

  private clientError(
    controlName: 'email' | 'password' | 'displayName',
  ): string | null {
    const control = this.form.controls[controlName];
    if (!control.errors) return null;
    if (control.errors['required']) {
      if (controlName === 'email') return 'auth.validation.emailRequired';
      if (controlName === 'password') return 'auth.validation.passwordRequired';
    }
    if (control.errors['email']) return 'auth.validation.emailInvalid';
    if (control.errors['minlength']) return 'auth.validation.passwordMin';
    if (control.errors['maxlength']) return 'auth.validation.displayNameMax';
    return null;
  }
}
