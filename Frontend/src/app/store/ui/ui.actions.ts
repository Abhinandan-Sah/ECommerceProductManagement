import { createAction, props } from '@ngrx/store';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export const showToast = createAction(
  '[UI] Show Toast',
  props<{ message: string; toastType: ToastType }>()
);
export const hideToast = createAction('[UI] Hide Toast');
export const setLoading = createAction(
  '[UI] Set Loading',
  props<{ isLoading: boolean }>()
);
