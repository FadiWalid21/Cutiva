import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const TOKEN_KEY = 'cutiva.accessToken';

/** Attaches Bearer token and redirects to login on 401. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Skip static i18n assets — no auth, avoids DI cycles during bootstrap.
  if (req.url.includes('/i18n/')) {
    return next(req);
  }

  const injector = inject(Injector);

  let token: string | null = null;
  try {
    token = localStorage.getItem(TOKEN_KEY);
  } catch {
    token = null;
  }

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !req.url.includes('/api/auth/login') &&
        !req.url.includes('/api/auth/forgot-password') &&
        !req.url.includes('/api/auth/reset-password')
      ) {
        injector.get(AuthService).handleUnauthorized();
      }
      return throwError(() => error);
    }),
  );
};
