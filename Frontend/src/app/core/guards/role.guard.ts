import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../../shared/models/user.model';

/**
 * Route guard that protects routes requiring specific roles.
 * 
 * Prevents users without required roles from accessing protected routes.
 * Redirects to login if not authenticated, or to unauthorized page if lacking required role.
 * 
 * Usage:
 * ```typescript
 * {
 *   path: 'admin',
 *   component: AdminComponent,
 *   canActivate: [authGuard, roleGuard],
 *   data: { roles: ['Admin'] }
 * }
 * ```
 * 
 * Validates: Requirements 8.4, 8.5, 8.6
 */
export const roleGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): boolean => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Check if user is authenticated
  const user = authService.getCurrentUser();
  if (!user) {
    // Not authenticated, redirect to login
    authService.redirectUrl = state.url;
    router.navigate(['/login']);
    return false;
  }

  // Get required roles from route data
  const requiredRoles = route.data['roles'] as string[];
  
  // If no roles specified, allow access (guard is misconfigured)
  if (!requiredRoles || requiredRoles.length === 0) {
    return true;
  }

  // Check if user has one of the required roles
  const normalizedUserRole = user.role?.toString().trim().toLowerCase();
  const hasRequiredRole = requiredRoles
    .map(role => role.trim().toLowerCase())
    .includes(normalizedUserRole);

  if (hasRequiredRole) {
    return true;
  }

  // User lacks required role, redirect to unauthorized page
  router.navigate(['/unauthorized']);
  return false;
};
