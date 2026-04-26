import { Injectable } from '@angular/core';

/**
 * Service for managing authentication tokens in browser storage.
 * Provides methods to save, retrieve, and clear access and refresh tokens.
 * 
 * Uses localStorage for persistent token storage across browser sessions.
 * 
 * Security Note: Tokens stored in localStorage are vulnerable to XSS attacks.
 * Consider using httpOnly cookies for production environments.
 */
@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private readonly ACCESS_TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  /**
   * Saves both access and refresh tokens to localStorage.
   * 
   * @param accessToken - The JWT access token
   * @param refreshToken - The JWT refresh token
   */
  saveTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  /**
   * Retrieves the access token from localStorage.
   * 
   * @returns The access token if it exists, null otherwise
   */
  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  /**
   * Retrieves the refresh token from localStorage.
   * 
   * @returns The refresh token if it exists, null otherwise
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Clears both access and refresh tokens from localStorage.
   * Used during logout or when tokens are invalidated.
   */
  clearTokens(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
  }
}
