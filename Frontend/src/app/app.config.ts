import {
  ApplicationConfig, provideZoneChangeDetection,
  ErrorHandler, isDevMode
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideToastr } from 'ngx-toastr';

import { routes } from './app.routes';
import { authInterceptor }  from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { tokenRefreshInterceptor } from './core/interceptors/token-refresh.interceptor';
import { GlobalErrorHandler } from './core/services/global-error-handler.service';
import { authReducer } from './store/auth/auth.reducer';
import { uiReducer }   from './store/ui/ui.reducer';
import { AuthEffects }  from './store/auth/auth.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withInterceptors([authInterceptor, tokenRefreshInterceptor, errorInterceptor, loadingInterceptor])
    ),
    provideAnimations(),
    provideToastr({
      positionClass:    'toast-top-right',
      maxOpened:        5,
      autoDismiss:      true,
      newestOnTop:      true,
      preventDuplicates: true,
      timeOut:          4000,
      extendedTimeOut:  1500,
      progressBar:      true,
      closeButton:      true,
    }),
    provideStore({ auth: authReducer, ui: uiReducer }),
    provideEffects([AuthEffects]),
    isDevMode()
      ? provideStoreDevtools({ maxAge: 25, logOnly: false })
      : [],
    { provide: ErrorHandler, useClass: GlobalErrorHandler }
  ]
};
