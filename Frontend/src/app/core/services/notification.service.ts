import { Injectable, inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

/**
 * Service for displaying notifications to users via ngx-toastr.
 *
 * Provides success, error, warning, and info toast notifications that
 * auto-dismiss and support progress bars. Replaces the previous
 * Angular Material Snackbar implementation.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly defaultDuration = 5000; // 5 seconds
  private readonly successDuration = 3000; // 3 seconds

  private toastr = inject(ToastrService);

  /**
   * Displays a success toast notification.
   * @param message - The success message to display
   * @param title   - Optional title (default: 'Success')
   */
  showSuccess(message: string, title: string = 'Success'): void {
    this.toastr.success(message, title, {
      timeOut: this.successDuration,
      progressBar: true,
      closeButton: true,
    });
  }

  /**
   * Displays an error toast notification.
   * @param message - The error message to display
   * @param title   - Optional title (default: 'Error')
   */
  showError(message: string, title: string = 'Error'): void {
    this.toastr.error(message, title, {
      timeOut: this.defaultDuration,
      progressBar: true,
      closeButton: true,
      disableTimeOut: false,
    });
  }

  /**
   * Displays a warning toast notification.
   * @param message - The warning message to display
   * @param title   - Optional title (default: 'Warning')
   */
  showWarning(message: string, title: string = 'Warning'): void {
    this.toastr.warning(message, title, {
      timeOut: this.defaultDuration,
      progressBar: true,
      closeButton: true,
    });
  }

  /**
   * Displays an info toast notification.
   * @param message - The info message to display
   * @param title   - Optional title (default: 'Info')
   */
  showInfo(message: string, title: string = 'Info'): void {
    this.toastr.info(message, title, {
      timeOut: this.defaultDuration,
      progressBar: true,
      closeButton: true,
    });
  }
}
