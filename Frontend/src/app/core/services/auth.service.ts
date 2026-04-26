import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  LoginRequest, LoginResponse, RegisterRequest,
  TokenResponse, ForgotPasswordRequest, ResetPasswordRequest,
  ChangePasswordRequest
} from '../../shared/models/auth.model';
import { User } from '../../shared/models/user.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly base = '/api/auth';

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.base}/login`, credentials);
  }

  register(data: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/register`, data);
  }

  logoutApi(refreshToken: string): Observable<void> {
    return this.http.post<void>(`${this.base}/logout`, { refreshToken });
  }

  refreshToken(refreshToken: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(
      `${this.base}/refresh`, { refreshToken }
    );
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.base}/forgot-password`, { email });
  }

  resetPassword(payload: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/reset-password`, payload);
  }

  changePassword(payload: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/change-password`, payload);
  }

  getProfile(): Observable<User> {
    return this.http.get<User>(`${this.base}/profile`);
  }
}
