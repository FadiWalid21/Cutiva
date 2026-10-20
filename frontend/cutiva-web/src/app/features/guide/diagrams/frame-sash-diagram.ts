import {
  AfterViewInit,
  Component,
  ElementRef,
  input,
  inject,
  viewChild,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { MotionService } from '../../../core/motion/motion.service';
import gsap from 'gsap';

@Component({
  selector: 'app-frame-sash-diagram',
  imports: [TranslatePipe],
  templateUrl: './frame-sash-diagram.html',
  styleUrl: './frame-sash-diagram.scss',
})
export class FrameSashDiagram implements AfterViewInit {
  readonly showLabels = input(true);
  private readonly motion = inject(MotionService);
  private readonly root = viewChild<ElementRef<HTMLElement>>('diagram');

  ngAfterViewInit(): void {
    const el = this.root()?.nativeElement;
    if (!el || this.motion.prefersReducedMotion()) return;
    const marks = el.querySelectorAll('.diag-anim');
    gsap.fromTo(
      marks,
      { opacity: 0 },
      { opacity: 1, duration: 0.55, stagger: 0.08, ease: 'power2.out', delay: 0.15 },
    );
  }
}
