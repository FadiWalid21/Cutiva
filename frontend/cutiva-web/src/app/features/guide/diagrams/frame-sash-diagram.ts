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
  selector: 'app-frame-sash-diagram',
  imports: [TranslatePipe],
  templateUrl: './frame-sash-diagram.html',
  styleUrl: './frame-sash-diagram.scss',
})
export class FrameSashDiagram implements AfterViewInit, OnDestroy {
  readonly showLabels = input(true);
  readonly stepKey = signal('guide.diagrams.stepFrameIdle');
  readonly playing = signal(false);

  private readonly motion = inject(MotionService);
  private readonly root = viewChild<ElementRef<HTMLElement>>('diagram');
  private tl: gsap.core.Timeline | null = null;
  private taught = false;

  constructor() {
    effect(() => {
      void this.showLabels();
      this.syncAnnotate();
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

    const frame = el.querySelector('.diag__frame');
    const sash = el.querySelector('.diag__sash');
    const glass = el.querySelector('.diag__glass');
    const dims = el.querySelectorAll('.diag-annotate.diag__dim');
    const clearance = el.querySelectorAll('.diag-annotate.diag__clearance');
    const labels = el.querySelectorAll('.diag-annotate.diag__labels');

    if (this.motion.prefersReducedMotion()) {
      gsap.set([frame, sash, glass, dims, clearance, labels], {
        opacity: 1,
      });
      this.applyLabelGate([dims, clearance, labels]);
      this.stepKey.set('guide.diagrams.stepFrame4');
      this.taught = true;
      this.playing.set(false);
      return;
    }

    const tl = gsap.timeline({
      onComplete: () => {
        this.taught = true;
        this.playing.set(false);
      },
    });
    this.tl = tl;

    tl.call(() => this.stepKey.set('guide.diagrams.stepFrame1'));
    tl.fromTo(frame, { opacity: 0 }, { opacity: 1, duration: 0.45 });
    tl.call(() => this.stepKey.set('guide.diagrams.stepFrame2'));
    tl.fromTo(
      [sash, glass],
      { opacity: 0 },
      { opacity: 1, duration: 0.45, stagger: 0.08 },
    );
    tl.call(() => this.stepKey.set('guide.diagrams.stepFrame3'));
    tl.fromTo(
      dims,
      { opacity: 0 },
      {
        opacity: this.showLabels() ? 1 : 0,
        duration: 0.4,
        stagger: 0.08,
      },
    );
    tl.fromTo(
      clearance,
      { opacity: 0 },
      { opacity: this.showLabels() ? 1 : 0, duration: 0.35 },
      '-=0.15',
    );
    tl.call(() => this.stepKey.set('guide.diagrams.stepFrame4'));
    tl.fromTo(
      labels,
      { opacity: 0 },
      { opacity: this.showLabels() ? 1 : 0, duration: 0.4, stagger: 0.05 },
    );
  }

  private resetPose(setIdle: boolean): void {
    const el = this.root()?.nativeElement;
    if (!el) return;
    this.taught = false;
    gsap.set(el.querySelector('.diag__frame'), { opacity: 0.25 });
    gsap.set(el.querySelector('.diag__sash'), { opacity: 0 });
    gsap.set(el.querySelector('.diag__glass'), { opacity: 0 });
    gsap.set(el.querySelectorAll('.diag-annotate'), { opacity: 0 });
    if (setIdle) this.stepKey.set('guide.diagrams.stepFrameIdle');
  }

  private applyLabelGate(groups: NodeListOf<Element>[]): void {
    const show = this.showLabels();
    for (const g of groups) {
      gsap.set(g, { opacity: show ? 1 : 0 });
    }
  }

  private syncAnnotate(): void {
    const el = this.root()?.nativeElement;
    if (!el || this.playing() || !this.taught) return;
    gsap.set(el.querySelectorAll('.diag-annotate'), {
      opacity: this.showLabels() ? 1 : 0,
      visibility: this.showLabels() ? 'visible' : 'hidden',
    });
  }
}
