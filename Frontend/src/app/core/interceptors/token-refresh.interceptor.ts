import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenStorageService } from '../services/token-storage.service';

// Track if we're currently refreshing to prevent multiple simultaneous refresh attempts
let isRefreshing = false;

export const tokenRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const tokenStorage = inject(TokenStorageService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only handle 401 errors
      if (error.status !== 401) {
        return throwError(() => error);
      }

      // Don't attempt refresh for auth endpoints
      if (isAuthEndpoint(req.url)) {
        return throwError(() => error);
      }

      // If already refreshing, wait and retry
      if (isRefreshing) {
        return throwError(() => error);
      }

      // Attempt token refresh
      const refreshToken = tokenStorage.getRefreshToken();
      if (!refreshToken) {
        // No refresh token available, redirect to login
        tokenStorage.clearTokens();
        router.navigate(['/login']);
        return throwError(() => error);
      }

      isRefreshing = true;

      return authService.refreshToken(refreshToken).pipe(
        switchMap((response) => {
          // Save new tokens
          tokenStorage.saveTokens(response.accessToken, response.refreshToken);
          isRefreshing = false;

          // Retry the original request with new token
          const clonedReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${response.accessToken}`
            }
          });
          return next(clonedReq);
        }),
        catchError((refreshError) => {
          // Refresh failed, clear tokens and redirect to login
          isRefreshing = false;
          tokenStorage.clearTokens();
          router.navigate(['/login']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};

function isAuthEndpoint(url: string): boolean {
  const authEndpoints = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh'];
  return authEndpoints.some(endpoint => url.includes(endpoint));
}
