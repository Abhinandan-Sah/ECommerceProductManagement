import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { Store } from '@ngrx/store';
import { filter, switchMap, map, take } from 'rxjs/operators';
import { selectUserRole, selectInitialized } from '../../store/auth/auth.selectors';
import { Role } from '../../shared/models/user.model';

export const roleGuard: CanActivateFn = (route, _state) => {
  const store  = inject(Store);
  const router = inject(Router);
  const requiredRoles = (route.data['roles'] ?? []) as Role[];

  // Wait until initAuth resolves before checking the user role
  return store.select(selectInitialized).pipe(
    filter(initialized => initialized === true),
    take(1),
    switchMap(() => store.select(selectUserRole).pipe(take(1))),
    map(userRole => {
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
