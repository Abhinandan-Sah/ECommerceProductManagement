import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

/**
 * Service for displaying notifications to users.
 * 
 * This service provides methods to show success, error, warning, and info messages
 * using Angular Material Snackbar. All notifications auto-dismiss after a configurable timeout.
 * 
 * Requirements: 14.5, 14.6
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly defaultDuration = 5000; // 5 seconds
  private readonly successDuration = 3000; // 3 seconds

  private snackBar = inject(MatSnackBar);

  /**
   * Displays a success message.
   * 
   * @param message - The success message to display
   * @param duration - Optional duration in milliseconds (default: 3000ms)
   */
  showSuccess(message: string, duration: number = this.successDuration): void {
    this.show(message, 'success-snackbar', duration);
  }

  /**
   * Displays an error message.
   * 
   * @param message - The error message to display
   * @param duration - Optional duration in milliseconds (default: 5000ms)
   */
  showError(message: string, duration: number = this.defaultDuration): void {
    this.show(message, 'error-snackbar', duration);
  }

  /**
   * Displays a warning message.
   * 
   * @param message - The warning message to display
   * @param duration - Optional duration in milliseconds (default: 5000ms)
   */
  showWarning(message: string, duration: number = this.defaultDuration): void {
    this.show(message, 'warning-snackbar', duration);
  }

  /**
   * Displays an info message.
   * 
   * @param message - The info message to display
   * @param duration - Optional duration in milliseconds (default: 5000ms)
   */
  showInfo(message: string, duration: number = this.defaultDuration): void {
    this.show(message, 'info-snackbar', duration);
  }

  /**
   * Internal method to display a snackbar with the specified configuration.
   * 
   * @param message - The message to display
   * @param panelClass - CSS class for styling the snackbar
   * @param duration - Duration in milliseconds before auto-dismiss
   */
  private show(message: string, panelClass: string, duration: number): void {
    const config: MatSnackBarConfig = {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: [panelClass]
    };

    this.snackBar.open(message, 'Close', config);
  }
}
