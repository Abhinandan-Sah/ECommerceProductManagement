import { createAction, props } from '@ngrx/store';
import { LoginResponse, TokenResponse } from '../../shared/models/auth.model';
import { User } from '../../shared/models/user.model';

export const login = createAction(
  '[Auth] Login',
  props<{ email: string; password: string }>()
);
export const loginSuccess = createAction(
  '[Auth] Login Success',
  props<{ response: LoginResponse }>()
);
export const loginFailure = createAction(
  '[Auth] Login Failure',
  props<{ error: string }>()
);
export const logout = createAction('[Auth] Logout');
export const logoutSuccess = createAction('[Auth] Logout Success');

export const refreshTokenAction = createAction(
  '[Auth] Refresh Token',
  props<{ refreshToken: string }>()
);
export const refreshSuccess = createAction(
  '[Auth] Refresh Success',
  props<{ response: TokenResponse }>()
);
export const refreshFailure = createAction('[Auth] Refresh Failure');

export const loadProfile = createAction('[Auth] Load Profile');
export const profileLoaded = createAction(
  '[Auth] Profile Loaded',
  props<{ user: User }>()
);
export const clearError = createAction('[Auth] Clear Error');
