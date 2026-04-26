import { AuthState } from './auth/auth.reducer';
import { UiState }   from './ui/ui.reducer';

export interface AppState {
  auth: AuthState;
  ui:   UiState;
}
