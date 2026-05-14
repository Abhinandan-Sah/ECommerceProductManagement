import {
  ApplicationConfig, provideZoneChangeDetection,
  ErrorHandler
} from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';

import { routes } from './app.routes';
import { authInterceptor }  from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { loadingInterceptor } from './core/interceptors/loading.interceptor';
import { tokenRefreshInterceptor } from './core/interceptors/token-refresh.interceptor';
import { GlobalErrorHandler } from './core/services/global-error-handler.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withInterceptors([authInterceptor, tokenRefreshInterceptor, errorInterceptor, loadingInterceptor])
    ),
    provideToastr({
      positionClass:    'toast-top-right',
      maxOpened:        5,
      autoDismiss:      true,
      newestOnTop:      true,
      preventDuplicates: true,
      timeOut:          12000,
      extendedTimeOut:  1500,
      progressBar:      true,
      closeButton:      true,
    }),
    { provide: ErrorHandler, useClass: GlobalErrorHandler }
  ]
};
