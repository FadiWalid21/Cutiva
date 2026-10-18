import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { routes } from './app.routes';
import { languageInterceptor } from './core/interceptors/language.interceptor';
import { LanguageService } from './core/services/language.service';
import { WorkshopContextService } from './core/services/workshop-context.service';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([languageInterceptor])),
    provideTranslateService({
      fallbackLang: environment.defaultLanguage,
      lang: environment.defaultLanguage,
    }),
    provideTranslateHttpLoader({
      prefix: './i18n/',
      suffix: '.json',
    }),
    provideAppInitializer(() => {
      inject(LanguageService).init();
      inject(WorkshopContextService).load();
    }),
  ],
};
