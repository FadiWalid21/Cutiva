import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import gsap from 'gsap';
import { ScrollTrigger } from 'gsap/ScrollTrigger';
import { PlansApi } from '../../core/api/plans.api';
import { MotionService } from '../../core/motion/motion.service';
import { AppLanguage, LanguageService } from '../../core/services/language.service';
import { PwaInstallService } from '../../core/services/pwa-install.service';
import { ThemeService } from '../../core/services/theme.service';
import {
  LANDING_CONTACT,
  LANDING_TIERS_FALLBACK,
  LandingTier,
  STORY_SCENES,
  StorySceneId,
  mapPublicPlansToTiers,
} from './landing.pricing';

gsap.registerPlugin(ScrollTrigger);

@Component({
  selector: 'app-landing-page',
  imports: [RouterLink, TranslatePipe, DecimalPipe],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.scss',
})
export class LandingPage implements OnInit, AfterViewInit, OnDestroy {
  private readonly motion = inject(MotionService);
  private readonly languageService = inject(LanguageService);
  private readonly themeService = inject(ThemeService);
  private readonly translate = inject(TranslateService);
  private readonly plansApi = inject(PlansApi);
  private readonly host = inject(ElementRef<HTMLElement>);
  readonly pwa = inject(PwaInstallService);

  private readonly storyStage = viewChild<ElementRef<HTMLElement>>('storyStage');
  private readonly contactVisual = viewChild<ElementRef<HTMLElement>>('contactVisual');

  readonly tiers = signal<LandingTier[]>(LANDING_TIERS_FALLBACK);
  readonly contact = LANDING_CONTACT;
  readonly scenes = STORY_SCENES;
  readonly floatArrowIds = [1, 2, 3, 4, 5] as const;
  readonly year = new Date().getFullYear();
  readonly activeScene = signal<StorySceneId>('stock');
  readonly typedTitle = signal('');
  readonly typedCaption = signal('');
  readonly typing = signal(false);
  readonly theme = this.themeService.theme;

  private ctx: gsap.Context | null = null;
  private storyTl: gsap.core.Timeline | null = null;
  private typeToken = 0;

  currentLang(): AppLanguage {
    return this.languageService.currentLanguage();
  }

  setLanguage(lang: AppLanguage): void {
    void this.languageService.setLanguage(lang).then(() => {
      void this.typeSceneCopy(this.activeScene(), true);
    });
  }

  toggleTheme(): void {
    this.themeService.toggle();
  }

  installApp(): void {
    void this.pwa.promptInstall();
  }

  ngOnInit(): void {
    this.plansApi.list().subscribe({
      next: (plans) => this.tiers.set(mapPublicPlansToTiers(plans)),
      error: () => this.tiers.set(LANDING_TIERS_FALLBACK),
    });
  }

  ngAfterViewInit(): void {
    const root = this.host.nativeElement;
    root.classList.add('landing--ready');

    if (this.motion.prefersReducedMotion()) {
      gsap.set(root.querySelectorAll('[data-reveal], [data-hero]'), {
        opacity: 1,
        y: 0,
      });
      this.activeScene.set('cutPlan');
      this.showOnlyScene('cutPlan');
      void this.typeSceneCopy('cutPlan', true);
      return;
    }

    void this.typeSceneCopy('stock', true);

    this.ctx = gsap.context(() => {
      gsap.fromTo(
        root.querySelectorAll('[data-hero]'),
        { opacity: 0, y: 28 },
        {
          opacity: 1,
          y: 0,
          duration: 1.05,
          stagger: 0.1,
          ease: 'power3.out',
          delay: 0.06,
        },
      );

      this.setupHeroArrows(root);

      gsap.utils.toArray<HTMLElement>(root.querySelectorAll('[data-reveal]')).forEach((el) => {
        gsap.fromTo(
          el,
          { opacity: 0, y: 36 },
          {
            opacity: 1,
            y: 0,
            duration: 0.9,
            ease: 'power3.out',
            scrollTrigger: {
              trigger: el,
              start: 'top 88%',
              toggleActions: 'play none none none',
            },
          },
        );
      });

      this.setupStoryLoop();
      this.setupContactMotion();
    }, root);
  }

  private setupHeroArrows(root: HTMLElement): void {
    const arrows = root.querySelectorAll<HTMLElement>('.landing__float-arrow');
    if (!arrows.length) return;

    arrows.forEach((arrow, i) => {
      const radiusX = 40 + i * 18;
      const radiusY = 28 + i * 14;
      const duration = 14 + i * 3.5;
      const startAngle = (i / arrows.length) * Math.PI * 2;
      const orbit = { t: startAngle };

      gsap.set(arrow, {
        x: Math.cos(startAngle) * radiusX,
        y: Math.sin(startAngle) * radiusY,
        rotation: (startAngle * 180) / Math.PI + 90,
        force3D: true,
      });

      gsap.to(orbit, {
        t: startAngle + Math.PI * 2,
        duration,
        ease: 'none',
        repeat: -1,
        onUpdate: () => {
          const pulse = 1 + 0.12 * Math.sin(orbit.t * 1.5);
          gsap.set(arrow, {
            x: Math.cos(orbit.t) * radiusX * pulse,
            y: Math.sin(orbit.t) * radiusY * pulse,
            rotation: (orbit.t * 180) / Math.PI + 90,
          });
        },
      });

      gsap.to(arrow, {
        opacity: 0.22 + (i % 3) * 0.08,
        duration: 2.8 + i * 0.4,
        yoyo: true,
        repeat: -1,
        ease: 'sine.inOut',
      });
    });
  }

  private setupContactMotion(): void {
    const visual = this.contactVisual()?.nativeElement;
    if (!visual) return;

    const ring = visual.querySelector('.contact-visual__ring');
    const arc = visual.querySelectorAll('.contact-visual__arc');
    const call = visual.querySelector('.contact-visual__call');
    const mail = visual.querySelector('.contact-visual__mail');

    if (ring) {
      gsap.to(ring, {
        scale: 1.08,
        opacity: 0.45,
        duration: 2.4,
        yoyo: true,
        repeat: -1,
        ease: 'sine.inOut',
        force3D: true,
      });
    }

    if (arc.length) {
      gsap.to(arc, {
        rotation: 360,
        duration: 18,
        ease: 'none',
        repeat: -1,
        transformOrigin: '50% 50%',
      });
    }

    if (call) {
      gsap.to(call, {
        y: -6,
        duration: 2.2,
        yoyo: true,
        repeat: -1,
        ease: 'sine.inOut',
      });
    }

    if (mail) {
      gsap.to(mail, {
        y: 6,
        duration: 2.6,
        yoyo: true,
        repeat: -1,
        ease: 'sine.inOut',
        delay: 0.3,
      });
    }
  }

  private showOnlyScene(id: StorySceneId): void {
    const stage = this.storyStage()?.nativeElement;
    if (!stage) return;
    stage.querySelectorAll<HTMLElement>('.story-scene').forEach((el) => {
      const match = el.dataset['scene'] === id;
      gsap.set(el, { opacity: match ? 1 : 0, force3D: true });
    });
  }

  private setupStoryLoop(): void {
    const stage = this.storyStage()?.nativeElement;
    if (!stage) return;

    const scenes = this.scenes.map((id) => ({
      id,
      el: stage.querySelector<HTMLElement>(`[data-scene="${id}"]`),
    }));

    scenes.forEach(({ el }, i) => {
      if (el) gsap.set(el, { opacity: i === 0 ? 1 : 0, force3D: true });
    });

    gsap.set(stage, { clearProps: 'transform,borderRadius,aspectRatio' });

    const hold = 3.2;
    const fade = 0.45;
    const tl = gsap.timeline({ repeat: -1, defaults: { ease: 'power2.inOut' } });

    const goTo = (index: number): void => {
      const prev = scenes[(index + scenes.length - 1) % scenes.length];
      const next = scenes[index];

      tl.add(() => {
        this.activeScene.set(next.id);
        void this.typeSceneCopy(next.id, false);
      });

      if (prev.el && next.el && prev.el !== next.el) {
        tl.to(prev.el, { opacity: 0, duration: fade, force3D: true }, '>');
        tl.to(next.el, { opacity: 1, duration: fade, force3D: true }, '<');
      }

      tl.to({}, { duration: hold });
    };

    this.activeScene.set('stock');
    tl.to({}, { duration: hold });
    goTo(1);
    goTo(2);
    goTo(3);
    goTo(0);

    this.storyTl = tl;
  }

  private async typeSceneCopy(id: StorySceneId, instant: boolean): Promise<void> {
    const token = ++this.typeToken;
    const title = this.translate.instant(`landing.product.scenes.${id}.title`) as string;
    const caption = this.translate.instant(`landing.product.scenes.${id}.caption`) as string;

    if (instant || this.motion.prefersReducedMotion()) {
      this.typedTitle.set(title);
      this.typedCaption.set(caption);
      this.typing.set(false);
      return;
    }

    this.typing.set(true);

    // Erase
    while (
      token === this.typeToken &&
      (this.typedTitle().length > 0 || this.typedCaption().length > 0)
    ) {
      this.typedCaption.set(this.typedCaption().slice(0, -2));
      this.typedTitle.set(this.typedTitle().slice(0, -2));
      await this.wait(12);
    }

    if (token !== this.typeToken) return;

    // Type title
    for (let i = 1; i <= title.length; i++) {
      if (token !== this.typeToken) return;
      this.typedTitle.set(title.slice(0, i));
      await this.wait(22);
    }

    // Type caption
    for (let i = 1; i <= caption.length; i++) {
      if (token !== this.typeToken) return;
      this.typedCaption.set(caption.slice(0, i));
      await this.wait(14);
    }

    if (token === this.typeToken) this.typing.set(false);
  }

  private wait(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  ngOnDestroy(): void {
    this.typeToken++;
    this.storyTl?.kill();
    this.ctx?.revert();
    ScrollTrigger.getAll().forEach((t) => {
      if (t.trigger && this.host.nativeElement.contains(t.trigger as Node)) {
        t.kill();
      }
    });
  }
}
