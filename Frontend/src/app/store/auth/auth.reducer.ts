import { createReducer, on } from '@ngrx/store';
import { User, Role } from '../../shared/models/user.model';
import * as AuthActions from './auth.actions';

export interface AuthState {
  user:            User | null;
  isAuthenticated: boolean;
  initialized:     boolean;   // true once initAuth has resolved (success or failure)
  loading:         boolean;
  error:           string | null;
}

const initialState: AuthState = {
  user:            null,
  isAuthenticated: false,
  initialized:     false,
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
    loading:         false,
    isAuthenticated: true,
    initialized:     true,   // mark as initialized so guards don't hang after login
    error:           null,
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
    initialized:     true,
    // Hydrate user from refresh response — state.user is null on page reload
    user: {
      id:        state.user?.id ?? '',
      fullName:  response.fullName,
      email:     response.email,
      role:      response.role as Role,
      isActive:  state.user?.isActive ?? true,
      createdAt: state.user?.createdAt ?? '',
      updatedAt: state.user?.updatedAt ?? null
    }
  })),

  on(AuthActions.profileLoaded, (state, { user }) => ({
    ...state, user
  })),

  on(AuthActions.logout, state => ({
    ...state, loading: true
  })),

  // Keep initialized:true after logout so guards don't hang on the next login attempt
  on(AuthActions.logoutSuccess, () => ({ ...initialState, initialized: true })),

  on(AuthActions.refreshFailure, () => ({ ...initialState, initialized: true })),

  on(AuthActions.clearError, state => ({
    ...state, error: null
  }))
);
