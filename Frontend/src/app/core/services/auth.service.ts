import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';

import { environment } from '../../../environments/environment';
import { TokenStorageService } from './token-storage.service';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  TokenResponse,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest
} from '../../shared/models/auth.model';
import { User, UserRole } from '../../shared/models/user.model';

/**
 * JWT token payload structure
 */
interface JwtPayload {
  sub?: string;          // User ID (standard JWT)
  email?: string;        // Email (custom/standard)
  role?: string;         // Role (custom)
  nameid?: string;       // NameIdentifier (some token providers)
  [key: string]: unknown;
  exp: number;           // Expiration timestamp
  iat: number;           // Issued at timestamp
}

/**
 * Service for managing authentication operations and token lifecycle.
 * 
 * Responsibilities:
 * - User authentication (register, login, logout)
 * - Token management (refresh, storage, validation)
 * - Password operations (forgot, reset, change)
 * - Authentication state management
 * - User profile retrieval
 * 
 * Uses BehaviorSubjects for reactive state management, allowing components
 * to subscribe to authentication state changes.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/api/auth`;
  
  // Authentication state management
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  
  // Store attempted URL for redirect after login
  public redirectUrl: string | null = null;

  constructor(
    private http: HttpClient,
    private tokenStorage: TokenStorageService,
    private router: Router
  ) {
    // Initialize authentication state from stored tokens
    this.initializeAuthState();
  }

  /**
   * Initialize authentication state on service creation.
   * Checks for existing tokens and updates state accordingly.
   */
  private initializeAuthState(): void {
    const token = this.tokenStorage.getAccessToken();
    if (token) {
      try {
        const user = this.decodeTokenToUser(token);
        this.currentUserSubject.next(user);
        this.isAuthenticatedSubject.next(true);
      } catch (error) {
        // Token is invalid, clear it
        this.clearAuthState();
      }
    }
  }

  /**
   * Register a new user account.
   * 
   * @param data - Registration data including email, password, firstName, lastName
   * @returns Observable that completes on successful registration
   */
  register(data: RegisterRequest): Observable<void> {
    const fullName = `${data.firstName} ${data.lastName}`.trim();
    return this.http.post<void>(`${this.apiUrl}/register`, {
      email: data.email,
      password: data.password,
      fullName
    });
  }

  /**
   * Authenticate user with email and password.
   * On success, stores tokens and updates authentication state.
   * 
   * @param credentials - Login credentials (email and password)
   * @returns Observable containing authentication tokens
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => this.handleLoginSuccess(response)),
        catchError(error => this.handleError(error))
      );
  }

  /**
   * Log out the current user.
   * Calls logout endpoint, clears tokens, and resets authentication state.
   * 
   * @returns Observable that completes on successful logout
   */
  logout(): Observable<void> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    
    // Always clear local state, even if API call fails
    const logoutObservable = refreshToken
      ? this.http.post<void>(`${this.apiUrl}/logout`, { refreshToken })
      : throwError(() => new Error('No refresh token available'));
    
    return logoutObservable.pipe(
      tap(() => this.handleLogoutSuccess()),
      catchError(error => {
        // Clear state even on error
        this.handleLogoutSuccess();
        return throwError(() => error);
      })
    );
  }

  /**
   * Refresh the access token using the refresh token.
   * Updates stored tokens and authentication state.
   * 
   * @param refreshToken - The refresh token
   * @returns Observable containing new authentication tokens
   */
  refreshToken(refreshToken: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/refresh`, { refreshToken })
      .pipe(
        tap(response => this.handleTokenRefresh(response)),
        catchError(error => this.handleError(error))
      );
  }

  /**
   * Initiate password reset flow by sending reset email.
   * 
   * @param email - User's email address
   * @returns Observable that completes when email is sent
   */
  forgotPassword(email: string): Observable<void> {
    const request: ForgotPasswordRequest = { email };
    return this.http.post<void>(`${this.apiUrl}/forgot-password`, request);
  }

  /**
   * Complete password reset with token and new password.
   * 
   * @param token - Password reset token from email
   * @param newPassword - New password
   * @returns Observable that completes on successful reset
   */
  resetPassword(token: string, newPassword: string): Observable<void> {
    const request: ResetPasswordRequest = { token, newPassword };
    return this.http.post<void>(`${this.apiUrl}/reset-password`, request);
  }

  /**
   * Change password for authenticated user.
   * 
   * @param currentPassword - Current password for verification
   * @param newPassword - New password
   * @returns Observable that completes on successful password change
   */
  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    const request: ChangePasswordRequest = { currentPassword, newPassword };
    return this.http.post<void>(`${this.apiUrl}/change-password`, request);
  }

  /**
   * Get current user profile information.
   * 
   * @returns Observable containing user profile data
   */
  getProfile(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/profile`);
  }

  /**
   * Get the current access token from storage.
   * 
   * @returns Access token if available, null otherwise
   */
  getAccessToken(): string | null {
    return this.tokenStorage.getAccessToken();
  }

  /**
   * Get the current refresh token from storage.
   * 
   * @returns Refresh token if available, null otherwise
   */
  getRefreshToken(): string | null {
    return this.tokenStorage.getRefreshToken();
  }

  /**
   * Check if user is currently authenticated.
   * 
   * @returns True if user has valid access token, false otherwise
   */
  isAuthenticated(): boolean {
    const token = this.tokenStorage.getAccessToken();
    if (!token) {
      return false;
    }
    
    try {
      const decoded = jwtDecode<JwtPayload>(token);
      const currentTime = Date.now() / 1000;
      return decoded.exp > currentTime;
    } catch (error) {
      return false;
    }
  }

  /**
   * Get the current user from authentication state.
   * 
   * @returns Current user if authenticated, null otherwise
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Get the role of the current user.
   * 
   * @returns User role if authenticated, null otherwise
   */
  getUserRole(): UserRole | null {
    const user = this.getCurrentUser();
    return user ? user.role : null;
  }

  /**
   * Decode JWT token to extract payload.
   * 
   * @param token - JWT token string
   * @returns Decoded token payload
   */
  decodeToken(token: string): JwtPayload {
    return jwtDecode<JwtPayload>(token);
  }

  /**
   * Clear authentication state and navigate to login.
   * Used when tokens are invalid or expired.
   */
  clearAuthState(): void {
    this.tokenStorage.clearTokens();
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  /**
   * Handle successful login response.
   * Stores tokens and updates authentication state.
   */
  private handleLoginSuccess(response: LoginResponse): void {
    this.tokenStorage.saveTokens(response.accessToken, response.refreshToken);
    
    try {
      const user = this.decodeTokenToUser(response.accessToken);
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
    } catch (error) {
      console.error('Failed to decode token:', error);
      this.clearAuthState();
    }
  }

  /**
   * Handle successful logout.
   * Clears tokens and resets authentication state.
   */
  private handleLogoutSuccess(): void {
    this.tokenStorage.clearTokens();
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/login']);
  }

  /**
   * Handle successful token refresh.
   * Updates stored tokens and authentication state.
   */
  private handleTokenRefresh(response: TokenResponse): void {
    this.tokenStorage.saveTokens(response.accessToken, response.refreshToken);
    
    try {
      const user = this.decodeTokenToUser(response.accessToken);
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
    } catch (error) {
      console.error('Failed to decode refreshed token:', error);
      this.clearAuthState();
    }
  }

  /**
   * Decode JWT token and convert to User object.
   * 
   * @param token - JWT token string
   * @returns User object with data from token
   */
  private decodeTokenToUser(token: string): User {
    const payload = this.decodeToken(token);
    const nameIdentifierClaim = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
    const emailClaim = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    const nameClaim = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    const userId = String(payload.sub ?? payload.nameid ?? nameIdentifierClaim ?? '');
    const email = String(payload.email ?? emailClaim ?? '');
    const fullName = String(payload['name'] ?? payload['unique_name'] ?? nameClaim ?? '').trim();
    const role = String(payload.role ?? roleClaim ?? UserRole.User) as UserRole;
    const { firstName, lastName } = this.splitFullName(fullName);
    
    return {
      id: userId,
      email,
      firstName,
      lastName,
      role,
      isActive: true, // Assume active if token is valid
      createdAt: '',
      updatedAt: ''
    };
  }

  /**
   * Split a full name value into first and last name parts.
   */
  private splitFullName(fullName: string): { firstName: string; lastName: string } {
    const normalized = fullName.trim();
    if (!normalized) {
      return { firstName: '', lastName: '' };
    }

    const parts = normalized.split(/\s+/);
    if (parts.length === 1) {
      return { firstName: parts[0], lastName: '' };
    }

    return {
      firstName: parts[0],
      lastName: parts.slice(1).join(' ')
    };
  }

  /**
   * Handle HTTP errors.
   * 
   * @param error - HTTP error response
   * @returns Observable error
   */
  private handleError(error: any): Observable<never> {
    console.error('Auth service error:', error);
    return throwError(() => error);
  }
}
