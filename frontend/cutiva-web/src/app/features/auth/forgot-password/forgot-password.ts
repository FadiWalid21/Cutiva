import { Component, OnInit, ElementRef, inject, signal, viewChild } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthApi } from '../../../core/api/auth.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { MotionService } from '../../../core/motion/motion.service';
import { FormFieldError } from '../../../shared/form-field/form-field-error';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, FormFieldError],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.scss',
})
export class ForgotPassword implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApi);
  private readonly motion = inject(MotionService);

  private readonly brand = viewChild<ElementRef<HTMLElement>>('brand');
  private readonly panel = viewChild<ElementRef<HTMLElement>>('panel');

  readonly step = signal<'request' | 'reset' | 'done'>('request');
  readonly submitting = signal(false);
  readonly showPassword = signal(false);
  readonly formError = signal<string | null>(null);
  readonly notice = signal<string | null>(null);
  readonly fieldErrors = signal<{
    email: string | null;
    code: string | null;
    newPassword: string | null;
  }>({ email: null, code: null, newPassword: null });

  readonly isI18nKey = isI18nKey;

  readonly requestForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  readonly resetForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  ngOnInit(): void {
    queueMicrotask(() => {
      this.motion.authPanelEnter(
        this.panel()?.nativeElement ?? null,
        this.brand()?.nativeElement ?? null,
      );
    });
  }

  requestCode(): void {
    this.requestForm.markAllAsTouched();
    this.formError.set(null);
    if (this.requestForm.invalid) {
      this.fieldErrors.update((e) => ({
        ...e,
        email: 'auth.validation.emailRequired',
      }));
      return;
    }

    const email = this.requestForm.getRawValue().email.trim();
    this.submitting.set(true);
    this.authApi.forgotPassword(email).subscribe({
      next: () => {
        this.submitting.set(false);
        this.resetForm.patchValue({ email });
        this.notice.set('auth.forgot.sent');
        this.step.set('reset');
      },
      error: (err) => {
        this.submitting.set(false);
        this.formError.set(apiErrorMessage(err, 'auth.errors.generic'));
      },
    });
  }

  submitReset(): void {
    this.resetForm.markAllAsTouched();
    this.formError.set(null);
    if (this.resetForm.invalid) {
      this.formError.set('auth.validation.passwordRequired');
      return;
    }

    const body = this.resetForm.getRawValue();
    this.submitting.set(true);
    this.authApi
      .resetPassword({
        email: body.email.trim(),
        code: body.code.trim(),
        newPassword: body.newPassword,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.step.set('done');
        },
        error: (err) => {
          this.submitting.set(false);
          this.formError.set(apiErrorMessage(err, 'auth.forgot.invalidCode'));
        },
      });
  }
}
