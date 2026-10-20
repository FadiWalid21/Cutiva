import { Injectable, OnDestroy, inject, signal } from '@angular/core';
import { FeedbackTrigger } from '../api/feedback.api';

const STORAGE_KEY = 'cutiva.feedback.v1';
const LOGIN_DELAY_MS = 5 * 60 * 1000;
const COOLDOWN_DAYS = 7;

interface FeedbackStorage {
  /** ISO date (UTC day) when prompt last appeared. */
  lastShownDay?: string;
  /** Epoch ms until which prompts stay suppressed. */
  cooldownUntilMs?: number;
}

@Injectable({ providedIn: 'root' })
export class FeedbackPromptService implements OnDestroy {
  readonly open = signal(false);
  readonly trigger = signal<FeedbackTrigger>('LoginIdle');

  private shownThisSession = false;
  private loginTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnDestroy(): void {
    this.clearLoginTimer();
  }

  /** Call once when authenticated shell loads. */
  startSessionWatch(): void {
    this.clearLoginTimer();
    if (!this.canPrompt()) return;

    this.loginTimer = setTimeout(() => {
      this.loginTimer = null;
      this.tryOpen('LoginIdle');
    }, LOGIN_DELAY_MS);
  }

  stopSessionWatch(): void {
    this.clearLoginTimer();
  }

  /** Call after a successful project calculate. */
  maybeShowAfterCalculate(): void {
    this.tryOpen('AfterCalculate');
  }

  dismissLater(): void {
    this.open.set(false);
    this.markCooldown();
  }

  markSubmitted(): void {
    this.open.set(false);
    this.markCooldown();
  }

  private tryOpen(trigger: FeedbackTrigger): void {
    if (this.shownThisSession || this.open() || !this.canPrompt()) return;

    this.shownThisSession = true;
    this.clearLoginTimer();
    this.trigger.set(trigger);
    this.open.set(true);
    this.markShownToday();
  }

  private canPrompt(): boolean {
    const state = this.read();
    const now = Date.now();
    if (state.cooldownUntilMs && state.cooldownUntilMs > now) return false;
    const today = this.utcDay(now);
    if (state.lastShownDay === today) return false;
    return true;
  }

  private markShownToday(): void {
    const state = this.read();
    state.lastShownDay = this.utcDay(Date.now());
    this.write(state);
  }

  private markCooldown(): void {
    const state = this.read();
    state.cooldownUntilMs = Date.now() + COOLDOWN_DAYS * 24 * 60 * 60 * 1000;
    state.lastShownDay = this.utcDay(Date.now());
    this.write(state);
  }

  private utcDay(ms: number): string {
    return new Date(ms).toISOString().slice(0, 10);
  }

  private read(): FeedbackStorage {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return {};
      return JSON.parse(raw) as FeedbackStorage;
    } catch {
      return {};
    }
  }

  private write(state: FeedbackStorage): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  }

  private clearLoginTimer(): void {
    if (this.loginTimer != null) {
      clearTimeout(this.loginTimer);
      this.loginTimer = null;
    }
  }
}
