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
  selector: 'app-clearance-diagram',
  imports: [TranslatePipe],
  templateUrl: './clearance-diagram.html',
  styleUrl: './clearance-diagram.scss',
})
export class ClearanceDiagram implements AfterViewInit {
  readonly showLabels = input(true);
  private readonly motion = inject(MotionService);
  private readonly root = viewChild<ElementRef<HTMLElement>>('diagram');

  ngAfterViewInit(): void {
    const el = this.root()?.nativeElement;
    if (!el || this.motion.prefersReducedMotion()) return;
    const arrows = el.querySelectorAll('.diag__gap');
    gsap.fromTo(
      arrows,
      { opacity: 0, scale: 0.85 },
      {
        opacity: 1,
        scale: 1,
        duration: 0.5,
        stagger: 0.12,
        ease: 'back.out(1.4)',
        transformOrigin: 'center',
      },
    );
  }
}
