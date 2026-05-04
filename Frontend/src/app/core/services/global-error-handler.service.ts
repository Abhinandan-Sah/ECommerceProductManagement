import { ErrorHandler, Injectable, inject } from '@angular/core';
import { NotificationService } from './notification.service';
import { environment } from '../../../environments/environment';

// Handles uncaught app errors
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private notificationService = inject(NotificationService);

  handleError(error: Error): void {
    if (!environment.production) {
      console.error('Global error caught:', error);
    }

    let errorMessage = 'An unexpected error occurred. Please try again.';
    
    if (error?.message) {
      errorMessage = error.message;
    }

    this.notificationService.showError(errorMessage);
  }
}
