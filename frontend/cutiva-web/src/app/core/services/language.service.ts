import { DOCUMENT, Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

export type AppLanguage = (typeof environment.supportedLanguages)[number];

const STORAGE_KEY = 'cutiva.lang';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly translate = inject(TranslateService);
  private readonly document = inject(DOCUMENT);

  readonly currentLanguage = signal<AppLanguage>(environment.defaultLanguage as AppLanguage);

  async init(): Promise<void> {
    const saved = this.readStoredLanguage();
    await this.setLanguage(saved ?? (environment.defaultLanguage as AppLanguage));
  }

  async setLanguage(lang: AppLanguage): Promise<void> {
    this.currentLanguage.set(lang);
    this.document.documentElement.lang = lang;
    this.document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
    localStorage.setItem(STORAGE_KEY, lang);
    await firstValueFrom(this.translate.use(lang));
  }

  private readStoredLanguage(): AppLanguage | null {
    const value = localStorage.getItem(STORAGE_KEY);
    if (value && (environment.supportedLanguages as readonly string[]).includes(value)) {
      return value as AppLanguage;
    }
    return null;
  }
}
