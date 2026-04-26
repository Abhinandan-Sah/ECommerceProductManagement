import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Route guard that protects routes requiring authentication.
 * 
 * Prevents unauthenticated users from accessing protected routes.
 * Stores the attempted URL for redirect after successful login.
 * 
 * Usage:
 * ```typescript
 * {
 *   path: 'profile',
 *   component: ProfileComponent,
 *   canActivate: [authGuard]
 * }
 * ```
 * 
 * Validates: Requirements 8.1, 8.2, 8.3
 */
export const authGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): boolean => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  // Store attempted URL for redirect after login
  authService.redirectUrl = state.url;
  
  // Redirect to login page
  router.navigate(['/login']);
  return false;
};
