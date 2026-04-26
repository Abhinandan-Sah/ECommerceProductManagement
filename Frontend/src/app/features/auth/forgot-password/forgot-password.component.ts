import { Component, OnInit } from '@angular/core';

import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingService } from '../../../core/services/loading.service';
import { emailValidator } from '../../../shared/utils/validators';

/**
 * ForgotPasswordComponent provides the user interface for initiating password reset.
 * 
 * Features:
 * - Reactive form with email validation
 * - Loading state during request
 * - Generic success message (prevents email enumeration)
 * - Error handling with user-friendly messages
 * - Link back to login page
 * 
 * Requirements: 5.1, 5.2, 5.3, 15.2
 */
@Component({
    selector: 'app-forgot-password',
    imports: [ReactiveFormsModule, RouterModule],
    templateUrl: './forgot-password.component.html',
    styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit {
  forgotPasswordForm!: FormGroup;
  isLoading = false;
  isSubmitted = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private loadingService: LoadingService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  /**
   * Initialize the forgot password form with validation rules.
   */
  private initializeForm(): void {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, emailValidator()]]
    });
  }

  /**
   * Handle form submission.
   * Validates form, calls forgot password service, and handles response.
   */
  onSubmit(): void {
    if (this.forgotPasswordForm.invalid) {
      this.markFormGroupTouched(this.forgotPasswordForm);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const email = this.forgotPasswordForm.value.email;

    this.authService.forgotPassword(email).subscribe({
      next: () => {
        this.isLoading = false;
        this.isSubmitted = true;
        this.notificationService.showSuccess('Password reset instructions sent!');
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error);
      }
    });
  }

  /**
   * Handle errors with user-friendly messages.
   * Note: We display generic messages to prevent email enumeration.
   */
  private handleError(error: any): void {
    if (error.status === 0) {
      this.errorMessage = 'Network error. Please check your connection.';
    } else {
      // Generic message for security (don't reveal if email exists)
      this.errorMessage = 'An error occurred. Please try again later.';
    }
    
    this.notificationService.showError(this.errorMessage);
  }

  /**
   * Mark all form controls as touched to trigger validation messages.
   */
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Check if a form field has errors and has been touched.
   */
  hasError(fieldName: string): boolean {
    const field = this.forgotPasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Get error message for a specific field.
   */
  getErrorMessage(fieldName: string): string {
    const field = this.forgotPasswordForm.get(fieldName);
    
    if (!field || !field.errors) {
      return '';
    }

    if (field.errors['required']) {
      return 'Email is required';
    }

    if (field.errors['invalidEmail']) {
      return 'Please enter a valid email address';
    }

    return 'Invalid input';
  }

  /**
   * Reset the form to allow another submission.
   */
  resetForm(): void {
    this.isSubmitted = false;
    this.forgotPasswordForm.reset();
  }
}
