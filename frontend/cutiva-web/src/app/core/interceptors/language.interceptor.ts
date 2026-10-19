import { HttpInterceptorFn } from '@angular/common/http';

const LANG_KEY = 'cutiva.lang';

/** Sends Accept-Language so the API can return ar/en messages. */
export const languageInterceptor: HttpInterceptorFn = (req, next) => {
  // Skip translation asset loads (avoids LanguageService ↔ HttpLoader cycles).
  if (req.url.includes('/i18n/')) {
    return next(req);
  }

  let language = 'en';
  try {
    const stored = localStorage.getItem(LANG_KEY);
    if (stored === 'en' || stored === 'ar') {
      language = stored;
    }
  } catch {
    /* ignore */
  }

  return next(
    req.clone({
      setHeaders: {
        'Accept-Language': language,
      },
    }),
  );
};
