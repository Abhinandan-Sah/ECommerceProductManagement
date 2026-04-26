import { ErrorHandler, Injectable } from '@angular/core';
import { NotificationService } from './notification.service';
import { environment } from '../../../environments/environment';

/**
 * Global error handler for uncaught errors.
 * 
 * Catches errors that are not handled by HTTP interceptors or component error handling.
 * Logs errors to console in development and displays user-friendly messages.
 * 
 * Requirements: 14.1, 14.2, 14.3
 */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private notificationService: NotificationService) {}

  handleError(error: Error): void {
    // Log error to console in development
    if (!environment.production) {
      console.error('Global error caught:', error);
    }

    // Extract error message
    let errorMessage = 'An unexpected error occurred. Please try again.';
    
    if (error?.message) {
      errorMessage = error.message;
    }

    // Display user-friendly error message
    this.notificationService.showError(errorMessage);

    // In production, you might want to send errors to a logging service
    // this.logErrorToService(error);
  }

  private logErrorToService(error: Error): void {
    // TODO: Implement error logging to external service (e.g., Sentry, LogRocket)
    // This would send error details to a monitoring service for production debugging
  }
}
