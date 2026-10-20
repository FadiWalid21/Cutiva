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
  selector: 'app-overlap-diagram',
  imports: [TranslatePipe],
  templateUrl: './overlap-diagram.html',
  styleUrl: './overlap-diagram.scss',
})
export class OverlapDiagram implements AfterViewInit {
  readonly showLabels = input(true);
  private readonly motion = inject(MotionService);
  private readonly root = viewChild<ElementRef<HTMLElement>>('diagram');

  ngAfterViewInit(): void {
    const el = this.root()?.nativeElement;
    if (!el || this.motion.prefersReducedMotion()) return;
    const zone = el.querySelector('.diag__overlap-zone');
    if (zone) {
      gsap.fromTo(
        zone,
        { opacity: 0.25 },
        {
          opacity: 0.9,
          duration: 0.9,
          yoyo: true,
          repeat: 2,
          ease: 'sine.inOut',
        },
      );
    }
  }
}
