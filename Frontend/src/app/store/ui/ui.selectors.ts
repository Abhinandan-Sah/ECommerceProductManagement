import { createFeatureSelector, createSelector } from '@ngrx/store';
import { UiState } from './ui.reducer';

export const selectUiState = createFeatureSelector<UiState>('ui');
export const selectIsLoading = createSelector(selectUiState, s => s.isLoading);
export const selectToast     = createSelector(selectUiState, s => s.toast);
