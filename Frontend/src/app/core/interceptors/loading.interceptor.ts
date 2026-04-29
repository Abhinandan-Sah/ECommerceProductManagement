import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

/**
 * URLs that should NEVER trigger the global loading overlay.
 * These are background/silent requests (auth handshakes, token refresh, etc.)
 * that would otherwise inflate the request counter unexpectedly.
 */
const SILENT_URLS = [
  '/api/auth/refresh',
  '/api/auth/logout',
  '/api/auth/profile',
];

function isSilentRequest(url: string): boolean {
  return SILENT_URLS.some(u => url.includes(u));
}

/**
 * HTTP interceptor for managing global loading state.
 *
 * Shows the loading overlay only for "user-visible" requests —
 * silent background calls (token refresh, logout, profile rehydration)
 * are intentionally skipped so the counter never gets stuck.
 */
export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);

  // Skip silent background requests entirely
  if (isSilentRequest(req.url)) {
    return next(req);
  }

  loadingService.show();

  return next(req).pipe(
    finalize(() => loadingService.hide())
  );
};
