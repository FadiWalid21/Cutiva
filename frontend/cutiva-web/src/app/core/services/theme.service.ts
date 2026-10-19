import { DOCUMENT, Injectable, inject, signal } from '@angular/core';

export type AppTheme = 'light' | 'dark';

const STORAGE_KEY = 'cutiva.theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  readonly theme = signal<AppTheme>('light');

  init(): void {
    const saved = localStorage.getItem(STORAGE_KEY);
    const theme: AppTheme =
      saved === 'dark' || saved === 'light' ? saved : 'light';
    this.setTheme(theme);
  }

  setTheme(theme: AppTheme): void {
    this.theme.set(theme);
    this.document.documentElement.dataset['theme'] = theme;
    localStorage.setItem(STORAGE_KEY, theme);
  }

  toggle(): void {
    this.setTheme(this.theme() === 'light' ? 'dark' : 'light');
  }
}
