import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

const SILENT_URLS = [
  '/api/auth/refresh',
  '/api/auth/logout',
  '/api/auth/profile',
];

function isSilentRequest(url: string): boolean {
  return SILENT_URLS.some(u => url.includes(u));
}

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);

  if (isSilentRequest(req.url)) {
    return next(req);
  }

  loadingService.show();

  return next(req).pipe(
    finalize(() => loadingService.hide())
  );
};
