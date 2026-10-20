import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MotionService } from '../../../core/motion/motion.service';
import { ClearanceDiagram } from '../diagrams/clearance-diagram';
import { FrameSashDiagram } from '../diagrams/frame-sash-diagram';
import { OverlapDiagram } from '../diagrams/overlap-diagram';

export type GuideSection =
  | 'overview'
  | 'recipes'
  | 'project'
  | 'resources'
  | 'cutting';

@Component({
  selector: 'app-guide-page',
  imports: [
    TranslatePipe,
    RouterLink,
    FrameSashDiagram,
    OverlapDiagram,
    ClearanceDiagram,
  ],
  templateUrl: './guide-page.html',
  styleUrl: './guide-page.scss',
})
export class GuidePage implements AfterViewInit {
  private readonly motion = inject(MotionService);
  private readonly root = viewChild<ElementRef<HTMLElement>>('pageRoot');
  private readonly panel = viewChild<ElementRef<HTMLElement>>('sectionPanel');

  readonly section = signal<GuideSection>('overview');
  readonly showDiagramLabels = signal(true);

  readonly sections: { id: GuideSection; labelKey: string }[] = [
    { id: 'overview', labelKey: 'guide.tabs.overview' },
    { id: 'recipes', labelKey: 'guide.tabs.recipes' },
    { id: 'project', labelKey: 'guide.tabs.project' },
    { id: 'resources', labelKey: 'guide.tabs.resources' },
    { id: 'cutting', labelKey: 'guide.tabs.cutting' },
  ];

  ngAfterViewInit(): void {
    this.motion.pageEnter(this.root()?.nativeElement ?? null);
  }

  setSection(id: GuideSection): void {
    this.section.set(id);
    queueMicrotask(() => {
      const el = this.panel()?.nativeElement;
      if (!el) return;
      this.motion.pageEnter(el);
      const blocks = el.querySelectorAll('.guide-block');
      this.motion.staggerIn(blocks);
    });
  }

  toggleLabels(): void {
    this.showDiagramLabels.update((v) => !v);
  }
}
