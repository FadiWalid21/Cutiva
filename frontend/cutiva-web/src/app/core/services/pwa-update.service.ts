import { Injectable, computed, inject, signal } from '@angular/core';
import { SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { filter } from 'rxjs';

const CHECK_INTERVAL_MS = 5 * 60 * 1000;

@Injectable({ providedIn: 'root' })
export class PwaUpdateService {
  private readonly swUpdate = inject(SwUpdate);

  private readonly updateReadySignal = signal(false);
  private readonly dismissedForSessionSignal = signal(false);
  private checkTimer: ReturnType<typeof setInterval> | null = null;

  readonly updateAvailable = computed(
    () => this.updateReadySignal() && !this.dismissedForSessionSignal(),
  );

  init(): void {
    if (!this.swUpdate.isEnabled) {
      return;
    }

    this.swUpdate.versionUpdates
      .pipe(filter((event): event is VersionReadyEvent => event.type === 'VERSION_READY'))
      .subscribe(() => {
        this.updateReadySignal.set(true);
        this.dismissedForSessionSignal.set(false);
      });

    void this.swUpdate.checkForUpdate().catch(() => undefined);

    this.checkTimer = setInterval(() => {
      void this.swUpdate.checkForUpdate().catch(() => undefined);
    }, CHECK_INTERVAL_MS);
  }

  dismissForSession(): void {
    this.dismissedForSessionSignal.set(true);
  }

  async activateUpdate(): Promise<void> {
    if (!this.swUpdate.isEnabled) {
      return;
    }

    try {
      await this.swUpdate.activateUpdate();
    } catch {
      // Still reload — browser may pick up the waiting worker on refresh.
    }
    document.location.reload();
  }
}
