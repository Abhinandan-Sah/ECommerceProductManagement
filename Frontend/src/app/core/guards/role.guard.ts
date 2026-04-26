import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { Store } from '@ngrx/store';
import { map, take } from 'rxjs/operators';
import { selectUserRole } from '../../store/auth/auth.selectors';
import { Role } from '../../shared/models/user.model';

export const roleGuard: CanActivateFn = (route, _state) => {
  const store  = inject(Store);
  const router = inject(Router);
  const requiredRoles = (route.data['roles'] ?? []) as Role[];

  return store.select(selectUserRole).pipe(
    take(1),
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
