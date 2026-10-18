import { HttpErrorResponse } from '@angular/common/http';

/** Prefer API `{ message }` body; otherwise fall back to an i18n key. */
export function apiErrorMessage(
  err: unknown,
  fallbackKey: string,
): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error;
    if (typeof body === 'string' && body.trim()) {
      return body;
    }
    if (body && typeof body === 'object' && 'message' in body) {
      const message = (body as { message?: unknown }).message;
      if (typeof message === 'string' && message.trim()) {
        return message;
      }
    }
  }
  return fallbackKey;
}

export function isI18nKey(value: string): boolean {
  return value.includes('.') && !value.includes(' ');
}
