import { Component, OnInit, inject } from '@angular/core';

import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { passwordStrengthValidator, passwordMatchValidator } from '../../../shared/utils/validators';

/**
 * ChangePasswordComponent allows authenticated users to change their password.
 *
 * Features:
 * - Reactive form with currentPassword, newPassword, confirmPassword fields
 * - Password validation (strength requirements)
 * - Password match validation
 * - Loading state during password change
 * - Success notification
 * - Handles incorrect current password errors (400)
 * - Cancel button to return to profile view
 *
 * Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 15.2
 */
@Component({
  selector: 'app-change-password',
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent implements OnInit {
  private fb                  = inject(FormBuilder);
  private authService         = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router              = inject(Router);

  changePasswordForm!: FormGroup;
  isLoading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.initializeForm();
  }

  /**
   * Initialize the change password form with validation rules.
   */
  private initializeForm(): void {
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword:     ['', [Validators.required, passwordStrengthValidator()]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: passwordMatchValidator('newPassword', 'confirmPassword')
    });
  }

  /**
   * Handle form submission.
   */
  onSubmit(): void {
    if (this.changePasswordForm.invalid) {
      this.changePasswordForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const currentPassword = this.changePasswordForm.value.currentPassword;
    const newPassword     = this.changePasswordForm.value.newPassword;
    const confirmPassword = this.changePasswordForm.value.confirmPassword;

    this.authService.changePassword({ currentPassword, newPassword, confirmPassword }).subscribe({
      next: () => {
        this.isLoading = false;
        this.notificationService.showSuccess('Password changed successfully!');
        this.router.navigate(['/profile']);
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error);
      }
    });
  }

  /**
   * Handle password change errors.
   */
  private handleError(error: any): void {
    if (error.status === 400) {
      this.errorMessage = 'Current password is incorrect. Please try again.';
      this.changePasswordForm.get('currentPassword')?.setErrors({ incorrect: true });
    } else if (error.status === 401) {
      this.errorMessage = 'Session expired. Please log in again.';
    } else if (error.status === 0) {
      this.errorMessage = 'Network error. Please check your connection.';
    } else {
      this.errorMessage = error.error?.message || 'Failed to change password. Please try again.';
    }

    this.notificationService.showError(this.errorMessage);
  }

  /**
   * Cancel password change and return to profile view.
   */
  cancel(): void {
    this.router.navigate(['/profile']);
  }

  /**
   * Check if a form field has errors and has been touched.
   */
  hasError(fieldName: string): boolean {
    const field = this.changePasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Check if form has password mismatch error.
   */
  hasPasswordMismatch(): boolean {
    const form = this.changePasswordForm;
    const confirmField = form.get('confirmPassword');
    return !!(form.errors?.['passwordMismatch'] && confirmField?.touched);
  }

  /**
   * Get error message for a specific field.
   */
  getErrorMessage(fieldName: string): string {
    const field = this.changePasswordForm.get(fieldName);

    if (!field || !field.errors) {
      return '';
    }

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }

    if (field.errors['passwordStrength']) {
      return 'Password must be at least 8 characters and contain uppercase, lowercase, number, and special character';
    }

    if (field.errors['incorrect']) {
      return 'Current password is incorrect';
    }

    return 'Invalid input';
  }

  /**
   * Get user-friendly label for form field.
   */
  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      currentPassword: 'Current password',
      newPassword:     'New password',
      confirmPassword: 'Confirm password'
    };
    return labels[fieldName] || fieldName;
  }
}
