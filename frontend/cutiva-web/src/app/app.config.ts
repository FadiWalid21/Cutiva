import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  inject,
  isDevMode,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideServiceWorker } from '@angular/service-worker';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { firstValueFrom } from 'rxjs';
import { environment } from '../environments/environment';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { languageInterceptor } from './core/interceptors/language.interceptor';
import { AuthService } from './core/services/auth.service';
import { LanguageService } from './core/services/language.service';
import { PwaInstallService } from './core/services/pwa-install.service';
import { ThemeService } from './core/services/theme.service';
import { WorkshopContextService } from './core/services/workshop-context.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([languageInterceptor, authInterceptor])),
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode() && environment.production,
      registrationStrategy: 'registerWhenStable:30000',
    }),
    provideTranslateService({
      fallbackLang: environment.defaultLanguage,
      lang: environment.defaultLanguage,
    }),
    provideTranslateHttpLoader({
      prefix: '/i18n/',
      suffix: '.json',
    }),
    provideAppInitializer(() => {
      // All inject() calls must run synchronously before any await.
      const theme = inject(ThemeService);
      const language = inject(LanguageService);
      const auth = inject(AuthService);
      const workshops = inject(WorkshopContextService);
      const pwa = inject(PwaInstallService);

      theme.init();
      pwa.init();

      return language.init().then(() =>
        firstValueFrom(auth.loadMe()).then((user) => {
          if ((user || auth.isAuthenticated()) && !auth.isAdmin()) {
            workshops.load();
          }
        }),
      );
    }),
  ],
};
