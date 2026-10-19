import { HttpErrorResponse } from '@angular/common/http';

export type FieldErrors = Record<string, string[]>;

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
    if (body && typeof body === 'object') {
      const message = (body as { message?: unknown }).message;
      if (typeof message === 'string' && message.trim()) {
        return message;
      }
      const title = (body as { title?: unknown }).title;
      if (typeof title === 'string' && title.trim() && err.status !== 400) {
        return title;
      }
    }
  }
  return fallbackKey;
}

/** Extract FluentValidation / ProblemDetails field errors map. */
export function apiFieldErrors(err: unknown): FieldErrors {
  if (!(err instanceof HttpErrorResponse) || !err.error || typeof err.error !== 'object') {
    return {};
  }

  const body = err.error as { errors?: unknown };
  const raw = body.errors;
  if (!raw || typeof raw !== 'object') {
    return {};
  }

  const result: FieldErrors = {};
  for (const [key, value] of Object.entries(raw as Record<string, unknown>)) {
    if (Array.isArray(value)) {
      const messages = value.filter((item): item is string => typeof item === 'string');
      if (messages.length) {
        result[normalizeFieldKey(key)] = messages;
      }
    } else if (typeof value === 'string' && value.trim()) {
      result[normalizeFieldKey(key)] = [value];
    }
  }
  return result;
}

export function firstFieldError(
  errors: FieldErrors,
  field: string,
): string | null {
  const messages = errors[normalizeFieldKey(field)];
  return messages?.[0] ?? null;
}

export function isI18nKey(value: string): boolean {
  return value.includes('.') && !value.includes(' ');
}

/** True for offline, timeouts, and gateway failures (502/503/504). */
export function isApiUnavailable(err: unknown): boolean {
  if (!(err instanceof HttpErrorResponse)) return false;
  if (err.status === 0) return true;
  return err.status === 502 || err.status === 503 || err.status === 504;
}

function normalizeFieldKey(key: string): string {
  if (!key) return key;
  return key.charAt(0).toLowerCase() + key.slice(1);
}
