import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { LanguageService } from '../services/language.service';

/** Sends Accept-Language so the API can return ar/en messages. */
export const languageInterceptor: HttpInterceptorFn = (req, next) => {
  const language = inject(LanguageService).currentLanguage();
  return next(
    req.clone({
      setHeaders: {
        'Accept-Language': language,
      },
    }),
  );
};
