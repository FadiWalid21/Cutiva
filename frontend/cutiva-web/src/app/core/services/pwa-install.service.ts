import { Injectable, computed, signal } from '@angular/core';

const BANNER_KEY = 'cutiva.pwaBannerDismissed';

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>;
}

export type InstallHelpKind = 'ios' | 'desktop';

@Injectable({ providedIn: 'root' })
export class PwaInstallService {
  private deferred: BeforeInstallPromptEvent | null = null;

  private readonly canInstallSignal = signal(false);
  private readonly isStandaloneSignal = signal(this.detectStandalone());
  private readonly bannerDismissedSignal = signal(this.readDismissed());
  private readonly helpKindSignal = signal<InstallHelpKind | null>(null);

  readonly canInstall = this.canInstallSignal.asReadonly();
  readonly isStandalone = this.isStandaloneSignal.asReadonly();
  readonly helpKind = this.helpKindSignal.asReadonly();
  readonly isIos = /iphone|ipad|ipod/i.test(navigator.userAgent);

  /** Top banner: web only, until dismissed. */
  readonly showBanner = computed(
    () => !this.isStandaloneSignal() && !this.bannerDismissedSignal(),
  );

  /** Nav / shell Install: stays after banner dismiss. */
  readonly showInstallControl = computed(() => !this.isStandaloneSignal());

  /** @deprecated use helpKind — kept for template compatibility */
  readonly showIosHelp = computed(() => this.helpKindSignal() === 'ios');

  init(): void {
    window.addEventListener('beforeinstallprompt', (event) => {
      event.preventDefault();
      this.deferred = event as BeforeInstallPromptEvent;
      this.canInstallSignal.set(true);
    });

    window.addEventListener('appinstalled', () => {
      this.deferred = null;
      this.canInstallSignal.set(false);
      this.dismissBanner();
      this.closeHelp();
    });

    window
      .matchMedia('(display-mode: standalone)')
      .addEventListener('change', () => {
        this.isStandaloneSignal.set(this.detectStandalone());
      });
  }

  async promptInstall(): Promise<boolean> {
    if (this.deferred) {
      await this.deferred.prompt();
      const choice = await this.deferred.userChoice;
      this.deferred = null;
      this.canInstallSignal.set(false);
      if (choice.outcome === 'accepted') {
        this.dismissBanner();
        this.closeHelp();
        return true;
      }
      return false;
    }

    this.helpKindSignal.set(this.isIos ? 'ios' : 'desktop');
    return false;
  }

  dismissBanner(): void {
    this.bannerDismissedSignal.set(true);
    try {
      localStorage.setItem(BANNER_KEY, '1');
    } catch {
      /* ignore */
    }
  }

  closeHelp(): void {
    this.helpKindSignal.set(null);
  }

  /** @deprecated use closeHelp */
  closeIosHelp(): void {
    this.closeHelp();
  }

  private detectStandalone(): boolean {
    const mq = window.matchMedia('(display-mode: standalone)').matches;
    const ios =
      'standalone' in navigator &&
      (navigator as Navigator & { standalone?: boolean }).standalone === true;
    return mq || !!ios;
  }

  private readDismissed(): boolean {
    try {
      return localStorage.getItem(BANNER_KEY) === '1';
    } catch {
      return false;
    }
  }
}
