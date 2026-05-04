import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenStorageService } from '../services/token-storage.service';
import { AuthStateService } from '../state/auth-state.service';

let isRefreshing = false;

export const tokenRefreshInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const tokenStorage = inject(TokenStorageService);
  const authState = inject(AuthStateService);

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
        authState.clearSession();
        return throwError(() => error);
      }

      isRefreshing = true;

      return authService.refreshToken(refreshToken).pipe(
        switchMap((response) => {
          // Save new tokens
          tokenStorage.saveTokens(response.accessToken, response.refreshToken);
          authState.applyTokenResponse(response);
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
          authState.clearSession();
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
