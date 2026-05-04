import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { toObservable } from '@angular/core/rxjs-interop';
import { filter, map, take } from 'rxjs/operators';
import { AuthStateService } from '../state/auth-state.service';
import { Role } from '../../shared/models/user.model';

export const roleGuard: CanActivateFn = (route, _state) => {
  const auth = inject(AuthStateService);
  const router = inject(Router);
  const requiredRoles = (route.data['roles'] ?? []) as Role[];

  // Wait until initAuth resolves before checking the user role
  return toObservable(auth.initialized).pipe(
    filter(initialized => initialized === true),
    take(1),
    map(() => {
      const userRole = auth.userRole();
      if (!userRole) {
        router.navigate(['/login']);
        return false;
      }
      if (requiredRoles.length === 0) return true;
      if (requiredRoles.includes(userRole as Role)) return true;
      router.navigate(['/unauthorized']);
      return false;
    })
  );
};
