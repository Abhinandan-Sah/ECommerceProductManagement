import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { Store } from '@ngrx/store';
import { filter, switchMap, map, take } from 'rxjs/operators';
import { selectIsAuthenticated, selectInitialized } from '../../store/auth/auth.selectors';

export const authGuard: CanActivateFn = (_route, state) => {
  const store  = inject(Store);
  const router = inject(Router);

  // Wait until initAuth has resolved (token refresh complete or failed),
  // then make a single decision based on the resulting isAuthenticated value.
  return store.select(selectInitialized).pipe(
    filter(initialized => initialized === true),
    take(1),
    switchMap(() => store.select(selectIsAuthenticated).pipe(take(1))),
    map(isAuthenticated => {
      if (isAuthenticated) return true;
      router.navigate(['/login'], {
        queryParams: { returnUrl: state.url }
      });
      return false;
    })
  );
};
