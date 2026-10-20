import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import gsap from 'gsap';
import { MotionService } from '../../../core/motion/motion.service';

@Component({
  selector: 'app-clearance-diagram',
  imports: [TranslatePipe],
  templateUrl: './clearance-diagram.html',
  styleUrl: './clearance-diagram.scss',
})
export class ClearanceDiagram implements AfterViewInit, OnDestroy {
  readonly showLabels = input(true);
  readonly stepKey = signal('guide.diagrams.stepClearanceIdle');
  readonly playing = signal(false);

  private readonly motion = inject(MotionService);
  private readonly root = viewChild<ElementRef<HTMLElement>>('diagram');
  private tl: gsap.core.Timeline | null = null;

  constructor() {
    effect(() => {
      // Re-apply visibility when label toggle changes mid-play.
      void this.showLabels();
      this.applyLabelVisibility();
    });
  }

  ngAfterViewInit(): void {
    this.resetPose(true);
  }

  ngOnDestroy(): void {
    this.tl?.kill();
  }

  play(): void {
    const el = this.root()?.nativeElement;
    if (!el) return;
    this.tl?.kill();
    this.playing.set(true);
    this.resetPose(false);

    const sash = el.querySelector('.diag__sash');
    const gapGlow = el.querySelector('.diag__gap-glow');
    const annotate = el.querySelectorAll('.diag-annotate');
    const formula = el.querySelector('.diag__formula-line');

    if (this.motion.prefersReducedMotion()) {
      gsap.set(sash, { attr: { x: 60, y: 50, width: 240, height: 120 } });
      gsap.set(gapGlow, { opacity: 0.55 });
      gsap.set(annotate, { opacity: this.showLabels() ? 1 : 0 });
      gsap.set(formula, { opacity: this.showLabels() ? 1 : 0 });
      this.stepKey.set('guide.diagrams.stepClearance3');
      this.playing.set(false);
      return;
    }

    gsap.set(annotate, { opacity: 0 });
    gsap.set(formula, { opacity: 0 });
    gsap.set(gapGlow, { opacity: 0 });
    gsap.set(sash, { attr: { x: 40, y: 30, width: 280, height: 160 } });

    const tl = gsap.timeline({
      onComplete: () => this.playing.set(false),
    });
    this.tl = tl;

    tl.call(() => this.stepKey.set('guide.diagrams.stepClearance1'));
    tl.to({}, { duration: 1.6 });
    tl.call(() => this.stepKey.set('guide.diagrams.stepClearance2'));
    tl.to(sash, {
      attr: { x: 60, y: 50, width: 240, height: 120 },
      duration: 1.45,
      ease: 'power2.inOut',
    });
    tl.to(gapGlow, { opacity: 0.55, duration: 0.7 }, '-=0.55');
    tl.to(
      annotate,
      {
        opacity: this.showLabels() ? 1 : 0,
        duration: 0.75,
        stagger: 0.12,
      },
      '-=0.25',
    );
    tl.to(
      annotate,
      {
        scale: 1.06,
        duration: 0.35,
        yoyo: true,
        repeat: 1,
        transformOrigin: 'center',
        stagger: 0.05,
      },
      '-=0.2',
    );
    tl.to({}, { duration: 1.5 });
    tl.call(() => this.stepKey.set('guide.diagrams.stepClearance3'));
    tl.to(formula, {
      opacity: this.showLabels() ? 1 : 0,
      duration: 0.7,
    });
    tl.to({}, { duration: 1.4 });
  }

  private resetPose(setIdleCopy: boolean): void {
    const el = this.root()?.nativeElement;
    if (!el) return;
    const sash = el.querySelector('.diag__sash');
    gsap.set(sash, { attr: { x: 40, y: 30, width: 280, height: 160 } });
    gsap.set(el.querySelector('.diag__gap-glow'), { opacity: 0 });
    gsap.set(el.querySelectorAll('.diag-annotate'), { opacity: 0 });
    gsap.set(el.querySelector('.diag__formula-line'), { opacity: 0 });
    if (setIdleCopy) {
      this.stepKey.set('guide.diagrams.stepClearanceIdle');
    }
  }

  private applyLabelVisibility(): void {
    const el = this.root()?.nativeElement;
    if (!el || this.playing()) return;
    const show = this.showLabels();
    const annotate = el.querySelectorAll('.diag-annotate');
    const formula = el.querySelector('.diag__formula-line');
    // If we already finished a play, keep annotations in sync with toggle.
    const hasShrunken =
      el.querySelector('.diag__sash')?.getAttribute('width') === '240';
    if (hasShrunken) {
      gsap.set(annotate, { opacity: show ? 1 : 0 });
      gsap.set(formula, { opacity: show ? 1 : 0 });
    }
  }
}
