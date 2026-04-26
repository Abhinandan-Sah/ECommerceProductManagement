import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { switchMap, map, catchError, tap } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { TokenStorageService } from '../../core/services/token-storage.service';
import * as AuthActions from './auth.actions';

@Injectable()
export class AuthEffects {
  private actions$ = inject(Actions);
  private authService = inject(AuthService);
  private tokenStorage = inject(TokenStorageService);
  private router = inject(Router);

  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      switchMap(({ email, password }) =>
        this.authService.login({ email, password }).pipe(
          tap(response =>
            this.tokenStorage.saveTokens(
              response.accessToken,
              response.refreshToken
            )
          ),
          map(response => AuthActions.loginSuccess({ response })),
          catchError(error =>
            of(AuthActions.loginFailure({
              error: error.error?.message
                  ?? error.message
                  ?? 'Login failed. Please try again.'
            }))
          )
        )
      )
    )
  );

  loginSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loginSuccess),
      tap(() => this.router.navigate(['/profile']))
    ),
    { dispatch: false }
  );

  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logout),
      switchMap(() => {
        const refreshToken = this.tokenStorage.getRefreshToken();
        const call$ = refreshToken
          ? this.authService.logoutApi(refreshToken)
          : of(void 0);
        return call$.pipe(
          tap(() => this.tokenStorage.clearTokens()),
          map(() => AuthActions.logoutSuccess()),
          catchError(() => {
            this.tokenStorage.clearTokens();
            return of(AuthActions.logoutSuccess());
          })
        );
      })
    )
  );

  logoutSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.logoutSuccess),
      tap(() => this.router.navigate(['/login']))
    ),
    { dispatch: false }
  );

  loadProfile$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loadProfile),
      switchMap(() =>
        this.authService.getProfile().pipe(
          map(user => AuthActions.profileLoaded({ user })),
          catchError(() => of(AuthActions.logoutSuccess()))
        )
      )
    )
  );
}
