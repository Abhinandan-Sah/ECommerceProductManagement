import {
  HttpInterceptorFn, HttpErrorResponse,
  HttpRequest, HttpEvent
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import {
  catchError, switchMap, throwError,
  BehaviorSubject, filter, take, Observable
} from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenStorageService } from '../services/token-storage.service';
import { logout, refreshSuccess, refreshFailure } from '../../store/auth/auth.actions';

let isRefreshing = false;
let refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService  = inject(AuthService);
  const tokenStorage = inject(TokenStorageService);
  const store        = inject(Store);
  const router       = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isAuthEndpoint(req.url)) {
        return handle401(req, next, authService, tokenStorage, store);
      }
      if (error.status === 403) {
        router.navigate(['/unauthorized']);
      }
      return throwError(() => error);
    })
  );
};

function isAuthEndpoint(url: string): boolean {
  return ['/login', '/register', '/refresh',
          '/forgot-password', '/reset-password']
    .some(e => url.includes(e));
}

function handle401(
  req: HttpRequest<unknown>,
  next: any,
  authService: AuthService,
  tokenStorage: TokenStorageService,
  store: Store
): Observable<HttpEvent<unknown>> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    const token = tokenStorage.getRefreshToken();
    if (!token) {
      isRefreshing = false;
      store.dispatch(logout());
      return throwError(() => new Error('No refresh token'));
    }

    return authService.refreshToken(token).pipe(
      switchMap((response): Observable<HttpEvent<unknown>> => {
        isRefreshing = false;
        tokenStorage.saveTokens(
          response.accessToken, response.refreshToken
        );
        store.dispatch(refreshSuccess({ response }));
        refreshTokenSubject.next(response.accessToken);
        return next(addToken(req, response.accessToken)) as Observable<HttpEvent<unknown>>;
      }),
      catchError(err => {
        isRefreshing = false;
        refreshTokenSubject.next(null);
        store.dispatch(logout());
        return throwError(() => err);
      })
    );
  }

  return refreshTokenSubject.pipe(
    filter(t => t !== null),
    take(1),
    switchMap(token =>
      next(addToken(req, token!)) as Observable<HttpEvent<unknown>>
    )
  );
}

function addToken(
  req: HttpRequest<unknown>, token: string
): HttpRequest<unknown> {
  return req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });
}
