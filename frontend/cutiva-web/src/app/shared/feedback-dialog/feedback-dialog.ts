import {
  Component,
  ElementRef,
  Injector,
  OnDestroy,
  afterNextRender,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import gsap from 'gsap';
import { FeedbackApi } from '../../core/api/feedback.api';
import { MotionService } from '../../core/motion/motion.service';
import { AuthService } from '../../core/services/auth.service';
import { FeedbackPromptService } from '../../core/services/feedback-prompt.service';
import { WorkshopContextService } from '../../core/services/workshop-context.service';
import { apiErrorMessage } from '../../core/http/api-error';

@Component({
  selector: 'app-feedback-dialog',
  imports: [FormsModule, TranslatePipe],
  templateUrl: './feedback-dialog.html',
  styleUrl: './feedback-dialog.scss',
})
export class FeedbackDialog implements OnDestroy {
  private readonly prompts = inject(FeedbackPromptService);
  private readonly api = inject(FeedbackApi);
  private readonly auth = inject(AuthService);
  private readonly workshops = inject(WorkshopContextService);
  private readonly motion = inject(MotionService);
  private readonly injector = inject(Injector);

  private readonly backdrop = viewChild<ElementRef<HTMLElement>>('fbBackdrop');
  private readonly panel = viewChild<ElementRef<HTMLElement>>('fbPanel');

  readonly open = this.prompts.open;
  readonly rating = signal(0);
  readonly comment = signal('');
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly thanks = signal(false);

  private thanksTimer: ReturnType<typeof setTimeout> | null = null;
  private enterTl: gsap.core.Timeline | null = null;
  private wasOpen = false;

  constructor() {
    effect(() => {
      const isOpen = this.open();
      if (isOpen && !this.wasOpen) {
        this.wasOpen = true;
        this.rating.set(0);
        this.comment.set('');
        this.error.set(null);
        this.thanks.set(false);
        this.saving.set(false);
        afterNextRender(() => this.playEnter(), { injector: this.injector });
      } else if (!isOpen && this.wasOpen) {
        this.wasOpen = false;
        this.killEnter();
      }
    });
  }

  ngOnDestroy(): void {
    if (this.thanksTimer) clearTimeout(this.thanksTimer);
    this.killEnter();
  }

  ownerName(): string {
    const u = this.auth.currentUser();
    return u?.displayName?.trim() || u?.email || '—';
  }

  companyName(): string {
    return this.workshops.selectedWorkshop()?.name?.trim() || '—';
  }

  setRating(value: number): void {
    this.rating.set(value);
  }

  later(): void {
    this.prompts.dismissLater();
  }

  submit(): void {
    const rating = this.rating();
    if (rating < 1 || rating > 5) {
      this.error.set('feedback.ratingRequired');
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.api
      .submit({
        rating,
        comment: this.comment().trim() || null,
        trigger: this.prompts.trigger(),
        workshopId: this.workshops.selectedWorkshopId(),
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.thanks.set(true);
          this.thanksTimer = setTimeout(() => this.prompts.markSubmitted(), 1400);
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(apiErrorMessage(err, 'feedback.submitError'));
        },
      });
  }

  private playEnter(): void {
    const backdrop = this.backdrop()?.nativeElement;
    const panel = this.panel()?.nativeElement;
    if (!backdrop || !panel) return;

    this.killEnter();

    if (this.motion.prefersReducedMotion()) {
      gsap.set(backdrop, { opacity: 1 });
      gsap.set(panel, { opacity: 1, y: 0, scale: 1 });
      return;
    }

    this.enterTl = gsap.timeline({ defaults: { ease: 'power2.out' } });
    this.enterTl.fromTo(
      backdrop,
      { opacity: 0 },
      { opacity: 1, duration: 0.28 },
      0,
    );
    this.enterTl.fromTo(
      panel,
      { opacity: 0, y: 28, scale: 0.94 },
      { opacity: 1, y: 0, scale: 1, duration: 0.45, clearProps: 'transform' },
      0.06,
    );
  }

  private killEnter(): void {
    this.enterTl?.kill();
    this.enterTl = null;
  }
}
