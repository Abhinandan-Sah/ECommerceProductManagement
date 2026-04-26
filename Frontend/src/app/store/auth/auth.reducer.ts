import { createReducer, on } from '@ngrx/store';
import { User, Role } from '../../shared/models/user.model';
import * as AuthActions from './auth.actions';

export interface AuthState {
  user:            User | null;
  isAuthenticated: boolean;
  loading:         boolean;
  error:           string | null;
}

const initialState: AuthState = {
  user:            null,
  isAuthenticated: false,
  loading:         false,
  error:           null
};

export const authReducer = createReducer(
  initialState,

  on(AuthActions.login, state => ({
    ...state, loading: true, error: null
  })),

  on(AuthActions.loginSuccess, (state, { response }) => ({
    ...state,
    loading: false,
    isAuthenticated: true,
    error: null,
    user: {
      id:        '',          // id comes from /profile call, not login response
      fullName:  response.fullName,
      email:     response.email,
      role:      response.role as Role,
      isActive:  true,
      createdAt: '',
      updatedAt: null
    }
  })),

  on(AuthActions.loginFailure, (state, { error }) => ({
    ...state, loading: false, error
  })),

  on(AuthActions.refreshSuccess, (state, { response }) => ({
    ...state,
    isAuthenticated: true,
    user: state.user
      ? { ...state.user, email: response.email,
          fullName: response.fullName, role: response.role as Role }
      : null
  })),

  on(AuthActions.profileLoaded, (state, { user }) => ({
    ...state, user
  })),

  on(AuthActions.logout, state => ({
    ...state, loading: true
  })),

  on(AuthActions.logoutSuccess, () => initialState),

  on(AuthActions.refreshFailure, () => initialState),

  on(AuthActions.clearError, state => ({
    ...state, error: null
  }))
);
