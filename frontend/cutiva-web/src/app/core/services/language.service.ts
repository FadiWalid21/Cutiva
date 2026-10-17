import { DOCUMENT, Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { environment } from '../../../environments/environment';

export type AppLanguage = (typeof environment.supportedLanguages)[number];

const STORAGE_KEY = 'cutiva.lang';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly translate = inject(TranslateService);
  private readonly document = inject(DOCUMENT);

  readonly currentLanguage = signal<AppLanguage>(environment.defaultLanguage as AppLanguage);

  init(): void {
    const saved = this.readStoredLanguage();
    this.setLanguage(saved ?? (environment.defaultLanguage as AppLanguage));
  }

  setLanguage(lang: AppLanguage): void {
    this.translate.use(lang);
    this.currentLanguage.set(lang);
    this.document.documentElement.lang = lang;
    this.document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
    localStorage.setItem(STORAGE_KEY, lang);
  }

  private readStoredLanguage(): AppLanguage | null {
    const value = localStorage.getItem(STORAGE_KEY);
    if (value && (environment.supportedLanguages as readonly string[]).includes(value)) {
      return value as AppLanguage;
    }
    return null;
  }
}
