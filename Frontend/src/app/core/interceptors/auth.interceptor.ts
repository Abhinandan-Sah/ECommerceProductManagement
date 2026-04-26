import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TokenStorageService } from '../services/token-storage.service';

/**
 * HTTP interceptor that automatically attaches authentication tokens to outgoing requests.
 * 
 * This interceptor:
 * - Retrieves the access token from TokenStorageService
 * - Attaches the token to the Authorization header for authenticated requests
 * - Skips token attachment for public auth endpoints (login, register, refresh, forgot-password)
 * 
 * The interceptor uses Angular's functional interceptor pattern (HttpInterceptorFn).
 * 
 * @param req - The outgoing HTTP request
 * @param next - The next handler in the interceptor chain
 * @returns Observable of the HTTP event
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorageService);
  
  // List of endpoints that should not have tokens attached
  const publicEndpoints = [
    '/api/auth/login',
    '/api/auth/register',
    '/api/auth/refresh',
    '/api/auth/forgot-password',
    '/api/auth/reset-password'
  ];
  
  // Check if the request URL matches any public endpoint
  const isPublicEndpoint = publicEndpoints.some(endpoint => 
    req.url.includes(endpoint)
  );
  
  // Skip token attachment for public endpoints
  if (isPublicEndpoint) {
    return next(req);
  }
  
  // Get the access token from storage
  const token = tokenStorage.getAccessToken();
  
  // If token exists, clone the request and add Authorization header
  if (token) {
    const clonedReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(clonedReq);
  }
  
  // No token available, proceed with original request
  return next(req);
};
