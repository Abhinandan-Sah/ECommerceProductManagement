import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { toObservable } from '@angular/core/rxjs-interop';
import { filter, map, take } from 'rxjs/operators';
import { AuthStateService } from '../state/auth-state.service';

export const authGuard: CanActivateFn = (_route) => {
  const auth = inject(AuthStateService);
  const router = inject(Router);

  // Wait until initAuth has resolved (token refresh complete or failed),
  // then make a single decision based on the resulting isAuthenticated value.
  return toObservable(auth.initialized).pipe(
    filter(initialized => initialized === true),
    take(1),
    map(() => {
      if (auth.isAuthenticated()) return true;
      router.navigate(['/login']);
      return false;
    })
  );
};
