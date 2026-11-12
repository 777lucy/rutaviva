import { HttpErrorResponse } from '@angular/common/http';

const FALLBACK_TITLE = 'No se pudo completar la operación.';

export function problemTitle(error: unknown): string {
  if (error instanceof HttpErrorResponse && typeof error.error?.title === 'string') {
    return error.error.title;
  }

  return FALLBACK_TITLE;
}
