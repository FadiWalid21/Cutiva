import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MotionService } from '../../core/motion/motion.service';
import { AuthService } from '../../core/services/auth.service';
import { PlanEntitlementsService } from '../../core/services/plan-entitlements.service';
import { AppLanguage, LanguageService } from '../../core/services/language.service';
import { PwaInstallService } from '../../core/services/pwa-install.service';
import { ThemeService } from '../../core/services/theme.service';
import { WorkshopContextService } from '../../core/services/workshop-context.service';

const SIDEBAR_KEY = 'cutiva.sidebarCollapsed';
const NARROW_MQ = '(max-width: 900px)';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell implements AfterViewInit, OnDestroy {
  private readonly languageService = inject(LanguageService);
  private readonly themeService = inject(ThemeService);
  private readonly auth = inject(AuthService);
  private readonly motion = inject(MotionService);
  readonly pwa = inject(PwaInstallService);
  readonly workshopContext = inject(WorkshopContextService);
  readonly entitlements = inject(PlanEntitlementsService);
  readonly canManageCatalog = this.auth.canManageCatalog;

  private readonly shellRoot = viewChild<ElementRef<HTMLElement>>('shellRoot');
  private readonly navLinks = viewChild<ElementRef<HTMLElement>>('navLinks');

  readonly currentLanguage = this.languageService.currentLanguage;
  readonly theme = this.themeService.theme;
  readonly currentUser = this.auth.currentUser;
  readonly subscription = computed(() => this.auth.currentUser()?.subscription ?? null);
  readonly subscriptionWarn = computed(() => {
    const sub = this.subscription();
    if (!sub) return true;
    if (sub.status !== 'Active') return true;
    return sub.daysRemaining <= 7;
  });
  readonly seatsLabel = computed(() => {
    const sub = this.subscription();
    if (!sub || sub.seatUsed == null) return null;
    return {
      used: sub.seatUsed,
      max: sub.maxUsers ?? null,
    };
  });
  readonly showInstall = this.pwa.showInstallControl;
  readonly mobileNavOpen = signal(false);
  /** Desktop preference only — ignored on narrow viewports. */
  private readonly collapsedPref = signal(localStorage.getItem(SIDEBAR_KEY) === '1');
  private readonly narrowViewport = signal(
    typeof window !== 'undefined' ? window.matchMedia(NARROW_MQ).matches : false,
  );
  private narrowMql: MediaQueryList | null = null;
  private readonly onNarrowChange = (event: MediaQueryListEvent) => {
    this.narrowViewport.set(event.matches);
    if (event.matches) {
      this.mobileNavOpen.set(false);
    }
  };

  /** Effective collapse for UI — always expanded on mobile/PWA drawer. */
  readonly collapsed = computed(
    () => !this.narrowViewport() && this.collapsedPref(),
  );

  ngAfterViewInit(): void {
    this.narrowMql = window.matchMedia(NARROW_MQ);
    this.narrowViewport.set(this.narrowMql.matches);
    this.narrowMql.addEventListener('change', this.onNarrowChange);

    this.motion.pageEnter(this.shellRoot()?.nativeElement ?? null);
    const nav = this.navLinks()?.nativeElement;
    if (nav && !this.collapsed()) {
      this.motion.staggerIn(nav.querySelectorAll('a'));
    }
  }

  ngOnDestroy(): void {
    this.narrowMql?.removeEventListener('change', this.onNarrowChange);
  }

  toggleSidebar(): void {
    if (this.narrowViewport()) {
      this.toggleMobileNav();
      return;
    }
    const next = !this.collapsedPref();
    this.collapsedPref.set(next);
    localStorage.setItem(SIDEBAR_KEY, next ? '1' : '0');
  }

  setLanguage(lang: AppLanguage): void {
    void this.languageService.setLanguage(lang);
  }

  toggleTheme(): void {
    this.themeService.toggle();
  }

  logout(): void {
    this.auth.logout();
  }

  installApp(): void {
    void this.pwa.promptInstall();
  }

  toggleMobileNav(): void {
    this.mobileNavOpen.update((v) => !v);
  }

  closeMobileNav(): void {
    this.mobileNavOpen.set(false);
  }
}
