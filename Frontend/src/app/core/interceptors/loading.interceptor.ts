import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

/**
 * HTTP interceptor for managing global loading state.
 * 
 * This interceptor:
 * - Shows loading indicator when an HTTP request starts
 * - Hides loading indicator when the request completes (success or error)
 * - Uses LoadingService to track multiple concurrent requests
 * 
 * The LoadingService maintains a request counter, so the loading indicator
 * remains visible as long as any request is active.
 * 
 * @param req - The outgoing HTTP request
 * @param next - The next handler in the interceptor chain
 * @returns Observable of the HTTP event
 */
export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);
  
  // Show loading indicator when request starts
  loadingService.show();
  
  // Hide loading indicator when request completes (success or error)
  return next(req).pipe(
    finalize(() => {
      loadingService.hide();
    })
  );
};
