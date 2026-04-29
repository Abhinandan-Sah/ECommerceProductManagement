import { Component, inject } from '@angular/core';

import {
  FormBuilder, Validators, ReactiveFormsModule, AbstractControl
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  emailValidator, passwordStrengthValidator, passwordMatchValidator
} from '../../../shared/utils/validators';
import { extractErrorMessage } from '../../../core/utils/error-utils';

/**
 * RegisterComponent provides the user interface for new user registration.
 *
 * Features:
 * - Reactive form with fullName (single field, not firstName/lastName)
 * - Email format and password strength validation
 * - Password match validation via group-level validator
 * - One-shot subscribe for registration (not NgRx state)
 *
 * Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 14.4, 15.2
 */
@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  private fb           = inject(FormBuilder);
  private authService  = inject(AuthService);
  private notify       = inject(NotificationService);
  private router       = inject(Router);

  isLoading  = false;
  errorMessage = '';
  emailConflictError = ''; // For displaying 409 conflict error near email field

  form = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    email:    ['', [Validators.required, emailValidator()]],
    password: ['', [Validators.required, passwordStrengthValidator()]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: passwordMatchValidator('password', 'confirmPassword') });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.isLoading = true;
    this.errorMessage = '';
    this.emailConflictError = '';

    this.authService.register({
      fullName: this.form.value.fullName!,
      email:    this.form.value.email!,
      password: this.form.value.password!
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.notify.showSuccess('Registered successfully! Please log in.');
        this.router.navigate(['/login'], {
          queryParams: { registered: 'true' }
        });
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading = false;
        const errorMsg = extractErrorMessage(err);
        
        // Handle 409 conflict error specifically for email field
        if (err.status === 409) {
          this.emailConflictError = errorMsg;
          this.errorMessage = '';
        } else {
          this.errorMessage = errorMsg;
          this.emailConflictError = '';
        }
        
        this.notify.showError(errorMsg);
      }
    });
  }

  isInvalid(field: string): boolean {
    const c = this.form.get(field);
    return !!(c?.invalid && (c.dirty || c.touched));
  }

  getError(field: string): string {
    const c = this.form.get(field);
    if (!c?.errors) return '';
    if (c.errors['required'])         return `${field} is required`;
    if (c.errors['maxlength'])        return 'Name too long (max 100 chars)';
    if (c.errors['invalidEmail'])     return 'Enter a valid email';
    if (c.errors['passwordStrength']) return 'Password is too weak';
    if (c.errors['passwordMismatch']) return 'Passwords do not match';
    return '';
  }
}
