import { createReducer, on } from '@ngrx/store';
import * as UiActions from './ui.actions';

export interface UiState {
  isLoading: boolean;
  toast: {
    message:   string;
    toastType: string;
    visible:   boolean;
  };
}

const initialState: UiState = {
  isLoading: false,
  toast: { message: '', toastType: 'info', visible: false }
};

export const uiReducer = createReducer(
  initialState,
  on(UiActions.setLoading, (state, { isLoading }) => ({
    ...state, isLoading
  })),
  on(UiActions.showToast, (state, { message, toastType }) => ({
    ...state, toast: { message, toastType, visible: true }
  })),
  on(UiActions.hideToast, state => ({
    ...state, toast: { ...state.toast, visible: false }
  }))
);
