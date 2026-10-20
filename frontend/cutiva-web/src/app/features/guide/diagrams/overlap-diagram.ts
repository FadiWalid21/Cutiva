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
  selector: 'app-overlap-diagram',
  imports: [TranslatePipe],
  templateUrl: './overlap-diagram.html',
  styleUrl: './overlap-diagram.scss',
})
export class OverlapDiagram implements AfterViewInit, OnDestroy {
  readonly showLabels = input(true);
  readonly stepKey = signal('guide.diagrams.stepOverlapIdle');
  readonly playing = signal(false);

  private readonly motion = inject(MotionService);
  private readonly root = viewChild<ElementRef<HTMLElement>>('diagram');
  private tl: gsap.core.Timeline | null = null;

  constructor() {
    effect(() => {
      void this.showLabels();
      this.syncAnnotateOpacity();
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

    const leaf2 = el.querySelector('.diag__leaf2');
    const zone = el.querySelector('.diag__overlap-zone');
    const annotate = el.querySelectorAll('.diag-annotate');

    if (this.motion.prefersReducedMotion()) {
      gsap.set(leaf2, { attr: { x: 226 } });
      gsap.set(zone, { attr: { width: 28 }, opacity: 1 });
      gsap.set(annotate, { opacity: this.showLabels() ? 1 : 0 });
      this.stepKey.set('guide.diagrams.stepOverlap3');
      this.playing.set(false);
      return;
    }

    const tl = gsap.timeline({ onComplete: () => this.playing.set(false) });
    this.tl = tl;

    tl.call(() => this.stepKey.set('guide.diagrams.stepOverlap1'));
    tl.to({}, { duration: 0.65 });
    tl.call(() => this.stepKey.set('guide.diagrams.stepOverlap2'));
    tl.to(leaf2, { attr: { x: 226 }, duration: 0.9, ease: 'power2.inOut' });
    tl.to(
      zone,
      { attr: { width: 28 }, opacity: 1, duration: 0.45, ease: 'power2.out' },
      '-=0.35',
    );
    tl.call(() => this.stepKey.set('guide.diagrams.stepOverlap3'));
    tl.to(annotate, {
      opacity: this.showLabels() ? 1 : 0,
      duration: 0.4,
      stagger: 0.05,
    });
  }

  private resetPose(setIdle: boolean): void {
    const el = this.root()?.nativeElement;
    if (!el) return;
    // Start: leaf2 abutting leaf1 with no overlap (x = 246)
    gsap.set(el.querySelector('.diag__leaf2'), { attr: { x: 246 } });
    gsap.set(el.querySelector('.diag__overlap-zone'), {
      attr: { width: 0 },
      opacity: 0,
    });
    gsap.set(el.querySelectorAll('.diag-annotate'), { opacity: 0 });
    if (setIdle) this.stepKey.set('guide.diagrams.stepOverlapIdle');
  }

  private syncAnnotateOpacity(): void {
    const el = this.root()?.nativeElement;
    if (!el || this.playing()) return;
    const zone = el.querySelector('.diag__overlap-zone');
    const overlapped = zone?.getAttribute('width') === '28';
    if (overlapped) {
      gsap.set(el.querySelectorAll('.diag-annotate'), {
        opacity: this.showLabels() ? 1 : 0,
      });
    }
  }
}
