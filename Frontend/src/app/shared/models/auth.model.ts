import { Role } from './user.model';

export interface LoginRequest {
  email:    string;
  password: string;
}

export interface LoginResponse {
  accessToken:  string;
  refreshToken: string;
  expiresAt:    string;
  email:        string;
  fullName:     string;
  role:         string;
}

export interface RegisterRequest {
  fullName:  string;
  email:     string;
  password:  string;
}

export interface TokenResponse {
  accessToken:  string;
  refreshToken: string;
  expiresAt:    string;
  email:        string;
  fullName:     string;
  role:         string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token:           string;
  newPassword:     string;
  confirmPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword:     string;
  confirmPassword: string;
}
