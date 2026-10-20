import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import gsap from 'gsap';
import { MotionService } from '../../../core/motion/motion.service';
import { LanguageService } from '../../../core/services/language.service';
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
export class GuidePage implements AfterViewInit, OnDestroy {
  private readonly motion = inject(MotionService);
  private readonly language = inject(LanguageService);
  private readonly root = viewChild<ElementRef<HTMLElement>>('pageRoot');
  private readonly panel = viewChild<ElementRef<HTMLElement>>('sectionPanel');
  private readonly flowRoot = viewChild<ElementRef<HTMLElement>>('flowRoot');
  private flowTl: gsap.core.Timeline | null = null;

  readonly section = signal<GuideSection>('overview');
  readonly showDiagramLabels = signal(true);
  readonly flowStep = signal(0);
  readonly isRtl = this.language.currentLanguage;

  readonly flowSteps = [
    'guide.overview.stepResources',
    'guide.overview.stepRecipes',
    'guide.overview.stepProject',
    'guide.overview.stepCalc',
    'guide.overview.stepCut',
  ] as const;

  readonly sections: { id: GuideSection; labelKey: string }[] = [
    { id: 'overview', labelKey: 'guide.tabs.overview' },
    { id: 'recipes', labelKey: 'guide.tabs.recipes' },
    { id: 'project', labelKey: 'guide.tabs.project' },
    { id: 'resources', labelKey: 'guide.tabs.resources' },
    { id: 'cutting', labelKey: 'guide.tabs.cutting' },
  ];

  ngAfterViewInit(): void {
    this.motion.pageEnter(this.root()?.nativeElement ?? null);
    queueMicrotask(() => this.playFlowTeach());
  }

  ngOnDestroy(): void {
    this.flowTl?.kill();
  }

  setSection(id: GuideSection): void {
    this.section.set(id);
    this.flowTl?.kill();
    this.flowTl = null;
    queueMicrotask(() => {
      const el = this.panel()?.nativeElement;
      if (!el) return;
      this.motion.pageEnter(el);
      const blocks = el.querySelectorAll('.guide-block');
      this.motion.staggerIn(blocks);
      if (id === 'overview') {
        this.playFlowTeach();
      }
    });
  }

  toggleLabels(): void {
    this.showDiagramLabels.update((v) => !v);
  }

  playFlowTeach(): void {
    this.flowTl?.kill();
    const root = this.flowRoot()?.nativeElement;
    if (!root) return;

    const steps = root.querySelectorAll<HTMLElement>('.guide-flow__step');
    const arrows = root.querySelectorAll<HTMLElement>('.guide-flow__arrow');
    gsap.set(steps, { clearProps: 'all' });
    gsap.set(arrows, { clearProps: 'opacity' });
    steps.forEach((s) => s.classList.remove('is-lit'));
    this.flowStep.set(0);

    if (this.motion.prefersReducedMotion()) {
      steps.forEach((s) => s.classList.add('is-lit'));
      this.flowStep.set(steps.length);
      return;
    }

    gsap.set(steps, { opacity: 0.35, scale: 0.96 });
    gsap.set(arrows, { opacity: 0.2 });

    const tl = gsap.timeline();
    this.flowTl = tl;
    steps.forEach((step, i) => {
      tl.to(
        step,
        {
          opacity: 1,
          scale: 1,
          duration: 0.55,
          ease: 'power2.out',
          onStart: () => {
            step.classList.add('is-lit');
            this.flowStep.set(i + 1);
          },
        },
        i * 1.1,
      );
      if (arrows[i]) {
        tl.to(arrows[i], { opacity: 1, duration: 0.4 }, i * 1.1 + 0.35);
      }
    });
  }
}
