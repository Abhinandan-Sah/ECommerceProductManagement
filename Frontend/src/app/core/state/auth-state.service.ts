import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { LoginResponse, TokenResponse } from '../../shared/models/auth.model';
import { Role, User } from '../../shared/models/user.model';
import { AuthService } from '../services/auth.service';
import { TokenStorageService } from '../services/token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthStateService {
  private authService = inject(AuthService);
  private tokenStorage = inject(TokenStorageService);
  private router = inject(Router);

  private userSignal = signal<User | null>(null);
  private initializedSignal = signal(false);
  private loadingSignal = signal(false);
  private errorSignal = signal<string | null>(null);

  readonly user = this.userSignal.asReadonly();
  readonly initialized = this.initializedSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userSignal());
  readonly userRole = computed(() => this.userSignal()?.role ?? null);
  readonly userFullName = computed(() => this.userSignal()?.fullName ?? null);

  initAuth(): void {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (!refreshToken) {
      this.resetAuth(true);
      return;
    }

    this.authService.refreshToken(refreshToken).pipe(
      tap(response => {
        this.tokenStorage.saveTokens(response.accessToken, response.refreshToken);
        this.applyTokenResponse(response);
      }),
      switchMap(() => this.loadProfile()),
      catchError(() => {
        this.tokenStorage.clearTokens();
        this.resetAuth(true);
        return of(null);
      })
    ).subscribe();
  }

  login(email: string, password: string): Observable<User | null> {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    return this.authService.login({ email, password }).pipe(
      tap(response => {
        this.tokenStorage.saveTokens(response.accessToken, response.refreshToken);
        this.applyLoginResponse(response);
        this.navigateAfterLogin();
      }),
      switchMap(() => this.loadProfile()),
      catchError(error => {
        this.loadingSignal.set(false);
        this.errorSignal.set(
          error.error?.message ?? error.message ?? 'Login failed. Please try again.'
        );
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    this.loadingSignal.set(true);
    const refreshToken = this.tokenStorage.getRefreshToken();
    const logout$ = refreshToken ? this.authService.logoutApi(refreshToken) : of(void 0);

    logout$.pipe(
      catchError(() => of(void 0))
    ).subscribe(() => this.completeLogout());
  }

  clearSession(redirectToLogin = true): void {
    this.tokenStorage.clearTokens();
    this.resetAuth(true);
    if (redirectToLogin) {
      this.router.navigate(['/login']);
    }
  }

  applyTokenResponse(response: TokenResponse): void {
    const current = this.userSignal();
    this.userSignal.set({
      id: current?.id ?? '',
      fullName: response.fullName,
      email: response.email,
      role: response.role as Role,
      isActive: current?.isActive ?? true,
      createdAt: current?.createdAt ?? '',
      updatedAt: current?.updatedAt ?? null
    });
    this.initializedSignal.set(true);
    this.errorSignal.set(null);
  }

  clearError(): void {
    this.errorSignal.set(null);
  }

  private loadProfile(): Observable<User | null> {
    return this.authService.getProfile().pipe(
      tap(user => {
        this.userSignal.set(user);
        this.initializedSignal.set(true);
        this.loadingSignal.set(false);
        this.errorSignal.set(null);
      }),
      map(user => user as User | null),
      catchError(() => {
        this.initializedSignal.set(true);
        this.loadingSignal.set(false);
        this.errorSignal.set(null);
        return of(this.userSignal());
      })
    );
  }

  private applyLoginResponse(response: LoginResponse): void {
    this.userSignal.set({
      id: '',
      fullName: response.fullName,
      email: response.email,
      role: response.role as Role,
      isActive: true,
      createdAt: '',
      updatedAt: null
    });
    this.initializedSignal.set(true);
    this.errorSignal.set(null);
  }

  private navigateAfterLogin(): void {
    this.router.navigate(['/dashboard']);
  }

  private completeLogout(): void {
    this.clearSession(true);
  }

  private resetAuth(initialized: boolean): void {
    this.userSignal.set(null);
    this.initializedSignal.set(initialized);
    this.loadingSignal.set(false);
    this.errorSignal.set(null);
  }
}
