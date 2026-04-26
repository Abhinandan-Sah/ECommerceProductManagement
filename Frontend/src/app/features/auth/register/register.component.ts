import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingService } from '../../../core/services/loading.service';
import { emailValidator, passwordStrengthValidator } from '../../../shared/utils/validators';

/**
 * RegisterComponent provides the user interface for new user registration.
 * 
 * Features:
 * - Reactive form with comprehensive validation
 * - Email format and password strength validation
 * - Loading state during registration
 * - Error handling with user-friendly messages
 * - Redirect to login after successful registration
 * - Link to login page for existing users
 * 
 * Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 14.4, 15.2
 */
@Component({
    selector: 'app-register',
    imports: [CommonModule, ReactiveFormsModule, RouterModule],
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private loadingService: LoadingService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  /**
   * Initialize the registration form with validation rules.
   */
  private initializeForm(): void {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, emailValidator()]],
      password: ['', [Validators.required, passwordStrengthValidator()]],
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]]
    });
  }

  /**
   * Handle form submission.
   * Validates form, calls registration service, and handles response.
   */
  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.markFormGroupTouched(this.registerForm);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const registrationData = {
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      firstName: this.registerForm.value.firstName,
      lastName: this.registerForm.value.lastName
    };

    this.authService.register(registrationData).subscribe({
      next: () => {
        this.isLoading = false;
        this.notificationService.showSuccess('Registration successful! Please log in.');
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.isLoading = false;
        this.handleRegistrationError(error);
      }
    });
  }

  /**
   * Handle registration errors with user-friendly messages.
   */
  private handleRegistrationError(error: any): void {
    if (error.status === 409) {
      this.errorMessage = 'An account with this email already exists. Please use a different email or log in.';
    } else if (error.status === 400) {
      const validationErrors = error.error?.errors;
      if (validationErrors && typeof validationErrors === 'object') {
        const firstError = Object.values(validationErrors)
          .flat()
          .find((message): message is string => typeof message === 'string' && message.length > 0);
        this.errorMessage = firstError || 'Invalid registration data. Please check your inputs.';
      } else {
        this.errorMessage = error.error?.message || 'Invalid registration data. Please check your inputs.';
      }
    } else if (error.status === 0) {
      this.errorMessage = 'Network error. Please check your connection.';
    } else {
      this.errorMessage = error.error?.message || 'An error occurred during registration. Please try again.';
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
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Get error message for a specific field.
   */
  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    
    if (!field || !field.errors) {
      return '';
    }

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }

    if (field.errors['invalidEmail']) {
      return 'Please enter a valid email address';
    }

    if (field.errors['minlength']) {
      const minLength = field.errors['minlength'].requiredLength;
      return `${this.getFieldLabel(fieldName)} must be at least ${minLength} characters`;
    }

    if (field.errors['passwordStrength']) {
      return this.getPasswordStrengthErrors(field.errors['passwordStrength']);
    }

    return 'Invalid input';
  }

  /**
   * Get detailed password strength error messages.
   */
  private getPasswordStrengthErrors(errors: any): string {
    const messages: string[] = [];
    
    if (errors['minLength']) {
      messages.push('at least 8 characters');
    }
    if (errors['requiresUppercase']) {
      messages.push('one uppercase letter');
    }
    if (errors['requiresLowercase']) {
      messages.push('one lowercase letter');
    }
    if (errors['requiresDigit']) {
      messages.push('one digit');
    }
    if (errors['requiresSpecialChar']) {
      messages.push('one special character');
    }

    return `Password must contain ${messages.join(', ')}`;
  }

  /**
   * Get user-friendly label for form field.
   */
  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      email: 'Email',
      password: 'Password',
      firstName: 'First name',
      lastName: 'Last name'
    };
    return labels[fieldName] || fieldName;
  }
}
