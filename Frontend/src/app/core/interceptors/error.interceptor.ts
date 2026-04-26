import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpEvent } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take, Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenStorageService } from '../services/token-storage.service';

/**
 * Flag to track if a token refresh is currently in progress.
 * Prevents multiple simultaneous refresh requests.
 */
let isRefreshing = false;

/**
 * Subject to queue requests while token refresh is in progress.
 * Emits the new access token once refresh completes.
 */
let refreshTokenSubject = new BehaviorSubject<string | null>(null);

/**
 * HTTP interceptor for global error handling and automatic token refresh.
 * 
 * This interceptor:
 * - Intercepts all HTTP error responses
 * - Handles 401 Unauthorized errors by attempting token refresh
 * - Retries the original request with the new token on successful refresh
 * - Redirects to login on refresh failure
 * - Queues requests during token refresh to prevent race conditions
 * - Extracts and formats error messages for display
 * 
 * Token Refresh Flow:
 * 1. Request fails with 401
 * 2. Check if refresh is already in progress
 * 3. If not, initiate refresh using refresh token
 * 4. On success, retry original request with new token
 * 5. On failure, clear auth state and redirect to login
 * 6. If refresh in progress, queue the request and retry when refresh completes
 * 
 * @param req - The outgoing HTTP request
 * @param next - The next handler in the interceptor chain
 * @returns Observable of the HTTP event
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const tokenStorage = inject(TokenStorageService);
  const router = inject(Router);
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors with token refresh
      if (error.status === 401 && !isAuthEndpoint(req.url)) {
        return handle401Error(req, next, authService, tokenStorage, router);
      }
      
      // For other errors, just pass them through
      // Error display will be handled by components or a notification service
      return throwError(() => error);
    })
  );
};

/**
 * Checks if the request URL is an authentication endpoint.
 * Auth endpoints should not trigger token refresh logic.
 * 
 * @param url - The request URL
 * @returns True if the URL is an auth endpoint, false otherwise
 */
function isAuthEndpoint(url: string): boolean {
  const authEndpoints = [
    '/api/auth/login',
    '/api/auth/register',
    '/api/auth/refresh',
    '/api/auth/forgot-password',
    '/api/auth/reset-password'
  ];
  
  return authEndpoints.some(endpoint => url.includes(endpoint));
}

/**
 * Handles 401 Unauthorized errors by attempting to refresh the access token.
 * 
 * If a refresh is not already in progress:
 * - Initiates token refresh using the refresh token
 * - On success: retries the original request with the new token
 * - On failure: clears auth state and redirects to login
 * 
 * If a refresh is already in progress:
 * - Queues the request
 * - Waits for the refresh to complete
 * - Retries the request with the new token
 * 
 * @param req - The original failed request
 * @param next - The next handler in the interceptor chain
 * @param authService - The authentication service
 * @param tokenStorage - The token storage service
 * @param router - The Angular router
 * @returns Observable of the HTTP event
 */
function handle401Error(
  req: HttpRequest<unknown>,
  next: any,
  authService: AuthService,
  tokenStorage: TokenStorageService,
  router: Router
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);
    
    const refreshToken = tokenStorage.getRefreshToken();
    
    if (!refreshToken) {
      // No refresh token available, clear state and redirect to login
      isRefreshing = false;
      authService.clearAuthState();
      return throwError(() => new Error('No refresh token available'));
    }
    
    return authService.refreshToken(refreshToken).pipe(
      switchMap((response) => {
        isRefreshing = false;
        refreshTokenSubject.next(response.accessToken);
        
        // Retry the original request with the new token
        return next(addToken(req, response.accessToken)) as Observable<HttpEvent<unknown>>;
      }),
      catchError((error) => {
        isRefreshing = false;
        refreshTokenSubject.next(null);
        
        // Refresh failed, clear auth state and redirect to login
        authService.clearAuthState();
        return throwError(() => error);
      })
    );
  } else {
    // A refresh is already in progress, queue this request
    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => {
        return next(addToken(req, token!)) as Observable<HttpEvent<unknown>>;
      })
    );
  }
}

/**
 * Clones the request and adds the Authorization header with the provided token.
 * 
 * @param req - The original request
 * @param token - The access token to add
 * @returns The cloned request with the Authorization header
 */
function addToken(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}
