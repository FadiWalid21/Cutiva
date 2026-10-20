import {
  Component,
  OnInit,
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
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { AuthApi } from '../../../core/api/auth.api';
import { apiErrorMessage, isI18nKey } from '../../../core/http/api-error';
import { MotionService } from '../../../core/motion/motion.service';
import { FormFieldError } from '../../../shared/form-field/form-field-error';

type ForgotStep = 'email' | 'otp' | 'password' | 'done';

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

  readonly step = signal<ForgotStep>('email');
  readonly submitting = signal(false);
  readonly showPassword = signal(false);
  readonly formError = signal<string | null>(null);
  readonly notice = signal<string | null>(null);
  readonly emailValue = signal('');
  readonly otpValue = signal('');
  readonly fieldErrors = signal<{
    email: string | null;
    code: string | null;
    newPassword: string | null;
  }>({ email: null, code: null, newPassword: null });

  readonly isI18nKey = isI18nKey;

  readonly emailForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  readonly otpForm = this.fb.nonNullable.group({
    code: [
      '',
      [Validators.required, Validators.minLength(6), Validators.maxLength(6)],
    ],
  });

  readonly passwordForm = this.fb.nonNullable.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
  });

  ngOnInit(): void {
    queueMicrotask(() => this.playEnter());
  }

  maskedEmail(): string {
    const email = this.emailValue();
    const at = email.indexOf('@');
    if (at <= 1) return email;
    const local = email.slice(0, at);
    const domain = email.slice(at);
    const visible = local.slice(0, Math.min(2, local.length));
    return `${visible}${'•'.repeat(Math.max(local.length - visible.length, 2))}${domain}`;
  }

  stepIndex(): number {
    switch (this.step()) {
      case 'email':
        return 1;
      case 'otp':
        return 2;
      case 'password':
      case 'done':
        return 3;
    }
  }

  requestCode(): void {
    this.emailForm.markAllAsTouched();
    this.formError.set(null);
    this.notice.set(null);
    this.fieldErrors.update((e) => ({ ...e, email: null }));

    if (this.emailForm.invalid) {
      this.fieldErrors.update((e) => ({
        ...e,
        email: 'auth.validation.emailRequired',
      }));
      return;
    }

    const email = this.emailForm.getRawValue().email.trim();
    this.submitting.set(true);
    this.authApi.forgotPassword(email).subscribe({
      next: () => {
        this.submitting.set(false);
        this.emailValue.set(email);
        this.otpForm.reset({ code: '' });
        this.notice.set('auth.forgot.sent');
        this.goStep('otp');
      },
      error: (err) => {
        this.submitting.set(false);
        if (err instanceof HttpErrorResponse && err.status === 404) {
          this.formError.set('auth.forgot.emailNotFound');
          return;
        }
        this.formError.set(apiErrorMessage(err, 'auth.errors.generic'));
      },
    });
  }

  resendCode(): void {
    const email = this.emailValue();
    if (!email) return;
    this.formError.set(null);
    this.submitting.set(true);
    this.authApi.forgotPassword(email).subscribe({
      next: () => {
        this.submitting.set(false);
        this.notice.set('auth.forgot.resent');
      },
      error: (err) => {
        this.submitting.set(false);
        if (err instanceof HttpErrorResponse && err.status === 404) {
          this.formError.set('auth.forgot.emailNotFound');
          this.goStep('email');
          return;
        }
        this.formError.set(apiErrorMessage(err, 'auth.errors.generic'));
      },
    });
  }

  continueFromOtp(): void {
    this.otpForm.markAllAsTouched();
    this.formError.set(null);
    this.fieldErrors.update((e) => ({ ...e, code: null }));

    if (this.otpForm.invalid) {
      this.fieldErrors.update((e) => ({
        ...e,
        code: 'auth.forgot.codeRequired',
      }));
      return;
    }

    this.otpValue.set(this.otpForm.getRawValue().code.trim());
    this.passwordForm.reset({ newPassword: '' });
    this.notice.set(null);
    this.goStep('password');
  }

  submitReset(): void {
    this.passwordForm.markAllAsTouched();
    this.formError.set(null);
    this.fieldErrors.update((e) => ({ ...e, newPassword: null }));

    if (this.passwordForm.invalid) {
      this.fieldErrors.update((e) => ({
        ...e,
        newPassword: 'auth.validation.passwordMin',
      }));
      return;
    }

    const newPassword = this.passwordForm.getRawValue().newPassword;
    this.submitting.set(true);
    this.authApi
      .resetPassword({
        email: this.emailValue(),
        code: this.otpValue(),
        newPassword,
      })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.goStep('done');
        },
        error: (err) => {
          this.submitting.set(false);
          this.formError.set(apiErrorMessage(err, 'auth.forgot.invalidCode'));
          // Bad OTP → send user back to enter code again.
          if (err instanceof HttpErrorResponse && err.status === 400) {
            this.goStep('otp');
          }
        },
      });
  }

  backToEmail(): void {
    this.formError.set(null);
    this.notice.set(null);
    this.goStep('email');
  }

  backToOtp(): void {
    this.formError.set(null);
    this.notice.set(null);
    this.goStep('otp');
  }

  private goStep(next: ForgotStep): void {
    this.step.set(next);
    queueMicrotask(() => this.playEnter());
  }

  private playEnter(): void {
    this.motion.authPanelEnter(
      this.panel()?.nativeElement ?? null,
      this.brand()?.nativeElement ?? null,
    );
  }
}
