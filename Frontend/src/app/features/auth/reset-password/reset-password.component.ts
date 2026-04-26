import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { passwordStrengthValidator, passwordMatchValidator } from '../../../shared/utils/validators';

/**
 * ResetPasswordComponent provides the user interface for completing password reset.
 *
 * Features:
 * - Extracts reset token from URL query parameters
 * - Reactive form with password validation and matching
 * - Loading state during request
 * - Error handling for invalid/expired tokens
 * - Redirect to login after successful reset
 *
 * Requirements: 5.4, 5.5, 5.6, 15.2
 */
@Component({
  selector: 'app-reset-password',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {
  private fb                   = inject(FormBuilder);
  private authService          = inject(AuthService);
  private notificationService  = inject(NotificationService);
  private router               = inject(Router);
  private route                = inject(ActivatedRoute);

  resetPasswordForm!: FormGroup;
  isLoading = false;
  errorMessage = '';
  token: string | null = null;
  tokenInvalid = false;

  ngOnInit(): void {
    this.extractTokenFromUrl();
    this.initializeForm();
  }

  /**
   * Extract reset token from URL query parameters.
   */
  private extractTokenFromUrl(): void {
    this.route.queryParams.subscribe(params => {
      this.token = params['token'];

      if (!this.token) {
        this.tokenInvalid = true;
        this.errorMessage = 'Invalid or missing reset token. Please request a new password reset.';
      }
    });
  }

  /**
   * Initialize the reset password form with validation rules.
   */
  private initializeForm(): void {
    this.resetPasswordForm = this.fb.group({
      newPassword:     ['', [Validators.required, passwordStrengthValidator()]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: [passwordMatchValidator('newPassword', 'confirmPassword')]
    });
  }

  /**
   * Handle form submission.
   */
  onSubmit(): void {
    if (this.resetPasswordForm.invalid || !this.token) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const newPassword     = this.resetPasswordForm.value.newPassword;
    const confirmPassword = this.resetPasswordForm.value.confirmPassword;

    this.authService.resetPassword({
      token: this.token,
      newPassword,
      confirmPassword
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.notificationService.showSuccess('Password reset successful! Please log in with your new password.');
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.isLoading = false;
        this.handleResetError(error);
      }
    });
  }

  /**
   * Handle reset password errors with user-friendly messages.
   */
  private handleResetError(error: any): void {
    if (error.status === 400) {
      this.errorMessage = 'Invalid or expired reset token. Please request a new password reset.';
      this.tokenInvalid = true;
    } else if (error.status === 0) {
      this.errorMessage = 'Network error. Please check your connection.';
    } else {
      this.errorMessage = error.error?.message || 'An error occurred while resetting your password. Please try again.';
    }

    this.notificationService.showError(this.errorMessage);
  }

  /**
   * Check if a form field has errors and has been touched.
   */
  hasError(fieldName: string): boolean {
    const field = this.resetPasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Get error message for a specific field.
   */
  getErrorMessage(fieldName: string): string {
    const field = this.resetPasswordForm.get(fieldName);

    if (!field || !field.errors) {
      return '';
    }

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }

    if (field.errors['passwordStrength']) {
      return this.getPasswordStrengthErrors(field.errors['passwordStrength']);
    }

    if (field.errors['passwordMismatch']) {
      return 'Passwords do not match';
    }

    return 'Invalid input';
  }

  /**
   * Get detailed password strength error messages.
   */
  private getPasswordStrengthErrors(errors: any): string {
    const messages: string[] = [];

    if (errors['minLength'])         messages.push('at least 8 characters');
    if (errors['requiresUppercase']) messages.push('one uppercase letter');
    if (errors['requiresLowercase']) messages.push('one lowercase letter');
    if (errors['requiresDigit'])     messages.push('one digit');
    if (errors['requiresSpecialChar']) messages.push('one special character');

    return `Password must contain ${messages.join(', ')}`;
  }

  /**
   * Get user-friendly label for form field.
   */
  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      newPassword:     'New password',
      confirmPassword: 'Confirm password'
    };
    return labels[fieldName] || fieldName;
  }

  /**
   * Navigate to forgot password page to request a new token.
   */
  requestNewToken(): void {
    this.router.navigate(['/forgot-password']);
  }
}
