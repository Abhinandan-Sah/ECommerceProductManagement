import { TestBed } from '@angular/core/testing';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('MatSnackBar', ['open']);

    TestBed.configureTestingModule({
      providers: [
        NotificationService,
        { provide: MatSnackBar, useValue: spy }
      ]
    });

    service = TestBed.inject(NotificationService);
    snackBarSpy = TestBed.inject(MatSnackBar) as jasmine.SpyObj<MatSnackBar>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('showSuccess', () => {
    it('should display success message with default duration', () => {
      const message = 'Operation successful';
      
      service.showSuccess(message);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        message,
        'Close',
        jasmine.objectContaining({
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        })
      );
    });

    it('should display success message with custom duration', () => {
      const message = 'Operation successful';
      const customDuration = 5000;
      
      service.showSuccess(message, customDuration);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        message,
        'Close',
        jasmine.objectContaining({
          duration: customDuration
        })
      );
    });
  });

  describe('showError', () => {
    it('should display error message with default duration', () => {
      const message = 'An error occurred';
      
      service.showError(message);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        message,
        'Close',
        jasmine.objectContaining({
          duration: 5000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        })
      );
    });

    it('should display error message with custom duration', () => {
      const message = 'An error occurred';
      const customDuration = 10000;
      
      service.showError(message, customDuration);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        message,
        'Close',
        jasmine.objectContaining({
          duration: customDuration
        })
      );
    });
  });

  describe('showWarning', () => {
    it('should display warning message with default duration', () => {
      const message = 'Warning: Please review';
      
      service.showWarning(message);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        message,
        'Close',
        jasmine.objectContaining({
          duration: 5000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
          panelClass: ['warning-snackbar']
        })
      );
    });

    it('should display warning message with custom duration', () => {
      const message = 'Warning: Please review';
      const customDuration = 7000;
      
      service.showWarning(message, customDuration);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        message,
        'Close',
        jasmine.objectContaining({
          duration: customDuration
        })
      );
    });
  });

  describe('showInfo', () => {
    it('should display info message with default duration', () => {
      const message = 'Information: New feature available';
      
      service.showInfo(message);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        message,
        'Close',
        jasmine.objectContaining({
          duration: 5000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
          panelClass: ['info-snackbar']
        })
      );
    });

    it('should display info message with custom duration', () => {
      const message = 'Information: New feature available';
      const customDuration = 4000;
      
      service.showInfo(message, customDuration);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        message,
        'Close',
        jasmine.objectContaining({
          duration: customDuration
        })
      );
    });
  });

  describe('multiple notifications', () => {
    it('should handle multiple notifications in sequence', () => {
      service.showSuccess('Success 1');
      service.showError('Error 1');
      service.showWarning('Warning 1');
      service.showInfo('Info 1');

      expect(snackBarSpy.open).toHaveBeenCalledTimes(4);
    });
  });
});
