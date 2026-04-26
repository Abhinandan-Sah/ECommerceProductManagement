import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';

  // accessToken lives in memory ONLY — never written to any storage
  private accessToken: string | null = null;

  saveTokens(accessToken: string, refreshToken: string): void {
    this.accessToken = accessToken;
    sessionStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  getRefreshToken(): string | null {
    return sessionStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  clearTokens(): void {
    this.accessToken = null;
    sessionStorage.removeItem(this.REFRESH_TOKEN_KEY);
  }

  hasTokens(): boolean {
    return this.accessToken !== null;
  }
}
