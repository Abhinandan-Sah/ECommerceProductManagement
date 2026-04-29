import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import {
  LoginRequest, LoginResponse, RegisterRequest,
  TokenResponse, ForgotPasswordRequest, ResetPasswordRequest,
  ChangePasswordRequest
} from '../../shared/models/auth.model';
import { User } from '../../shared/models/user.model';
import { NotificationService } from './notification.service';
import { extractErrorMessage } from '../utils/error-utils';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private readonly base = '/api/auth';

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.base}/login`, credentials).pipe(
      tap(() => this.notificationService.showSuccess('Login successful')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  register(data: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/register`, data).pipe(
      tap(() => this.notificationService.showSuccess('Registration successful')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  logoutApi(refreshToken: string): Observable<void> {
    return this.http.post<void>(`${this.base}/logout`, { refreshToken }).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  refreshToken(refreshToken: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(
      `${this.base}/refresh`, { refreshToken }
    ).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.base}/forgot-password`, { email }).pipe(
      tap(() => this.notificationService.showSuccess('Password reset email sent')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  resetPassword(payload: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/reset-password`, payload).pipe(
      tap(() => this.notificationService.showSuccess('Password reset successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  changePassword(payload: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/change-password`, payload).pipe(
      tap(() => this.notificationService.showSuccess('Password changed successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getProfile(): Observable<User> {
    return this.http.get<User>(`${this.base}/profile`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }
}
