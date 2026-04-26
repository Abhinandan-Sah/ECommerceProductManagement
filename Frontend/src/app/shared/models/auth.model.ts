/**
 * Authentication Models
 * 
 * These models define the data structures for authentication-related operations
 * including login, registration, token management, and password operations.
 */

/**
 * Request payload for user login
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Response payload for successful login
 */
export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

/**
 * Request payload for user registration
 */
export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

/**
 * Response payload containing authentication tokens
 */
export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

/**
 * Request payload for refreshing access token
 */
export interface RefreshTokenRequest {
  refreshToken: string;
}

/**
 * Request payload for initiating password reset
 */
export interface ForgotPasswordRequest {
  email: string;
}

/**
 * Request payload for completing password reset
 */
export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

/**
 * Request payload for changing password
 */
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
