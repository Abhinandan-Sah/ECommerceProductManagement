import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AuthState } from './auth.reducer';

export const selectAuthState =
  createFeatureSelector<AuthState>('auth');

export const selectCurrentUser =
  createSelector(selectAuthState, s => s.user);

export const selectIsAuthenticated =
  createSelector(selectAuthState, s => s.isAuthenticated);

export const selectUserRole =
  createSelector(selectAuthState, s => s.user?.role ?? null);

export const selectUserFullName =
  createSelector(selectAuthState, s => s.user?.fullName ?? null);

export const selectAuthLoading =
  createSelector(selectAuthState, s => s.loading);

export const selectAuthError =
  createSelector(selectAuthState, s => s.error);
