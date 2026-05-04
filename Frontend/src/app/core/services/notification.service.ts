import { Injectable, inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly defaultDuration = 5000; // 5 seconds
  private readonly successDuration = 3000; // 3 seconds

  private toastr = inject(ToastrService);

  showSuccess(message: string, title: string = 'Success'): void {
    this.toastr.success(message, title, {
      timeOut: this.successDuration,
      progressBar: true,
      closeButton: true,
    });
  }

  showError(message: string, title: string = 'Error'): void {
    this.toastr.error(message, title, {
      timeOut: this.defaultDuration,
      progressBar: true,
      closeButton: true,
      disableTimeOut: false,
    });
  }

  showWarning(message: string, title: string = 'Warning'): void {
    this.toastr.warning(message, title, {
      timeOut: this.defaultDuration,
      progressBar: true,
      closeButton: true,
    });
  }

  showInfo(message: string, title: string = 'Info'): void {
    this.toastr.info(message, title, {
      timeOut: this.defaultDuration,
      progressBar: true,
      closeButton: true,
    });
  }
}
