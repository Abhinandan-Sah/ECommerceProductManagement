import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { emailValidator } from '../../../shared/utils/validators';
import { User, UpdateProfileRequest } from '../../../shared/models/user.model';

/**
 * EditProfileComponent allows users to update their profile information.
 * 
 * Features:
 * - Reactive form with email, firstName, lastName fields
 * - Pre-populated with current profile data
 * - Form validation with inline error messages
 * - Loading state during profile fetch and update
 * - Success notification and profile refresh after update
 * - Handles email conflict errors (409)
 * - Cancel button to return to profile view
 * 
 * Requirements: 7.3, 7.4, 7.5, 7.6, 14.4, 15.2
 */
@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './edit-profile.component.html',
  styleUrls: ['./edit-profile.component.css']
})
export class EditProfileComponent implements OnInit {
  editForm!: FormGroup;
  isLoading = true;
  isSaving = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.loadProfile();
  }

  /**
   * Initialize the edit form with validation rules.
   */
  private initializeForm(): void {
    this.editForm = this.fb.group({
      email: ['', [Validators.required, emailValidator()]],
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]]
    });
  }

  /**
   * Load current profile data and populate the form.
   */
  private loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.userService.getProfile().subscribe({
      next: (user) => {
        this.populateForm(user);
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.handleLoadError(error);
      }
    });
  }

  /**
   * Populate form with user data.
   */
  private populateForm(user: User): void {
    this.editForm.patchValue({
      email: user.email,
      firstName: user.firstName,
      lastName: user.lastName
    });
  }

  /**
   * Handle form submission.
   */
  onSubmit(): void {
    if (this.editForm.invalid) {
      this.markFormGroupTouched(this.editForm);
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const updateData: UpdateProfileRequest = {
      email: this.editForm.value.email,
      firstName: this.editForm.value.firstName,
      lastName: this.editForm.value.lastName
    };

    this.userService.updateProfile(updateData).subscribe({
      next: () => {
        this.isSaving = false;
        this.notificationService.showSuccess('Profile updated successfully!');
        this.router.navigate(['/profile']);
      },
      error: (error) => {
        this.isSaving = false;
        this.handleUpdateError(error);
      }
    });
  }

  /**
   * Handle profile loading errors.
   */
  private handleLoadError(error: any): void {
    if (error.status === 401) {
      this.errorMessage = 'Session expired. Please log in again.';
    } else if (error.status === 0) {
      this.errorMessage = 'Network error. Please check your connection.';
    } else {
      this.errorMessage = error.error?.message || 'Failed to load profile. Please try again.';
    }
    
    this.notificationService.showError(this.errorMessage);
  }

  /**
   * Handle profile update errors.
   */
  private handleUpdateError(error: any): void {
    if (error.status === 409) {
      this.errorMessage = 'This email is already in use. Please choose a different email.';
      this.editForm.get('email')?.setErrors({ emailTaken: true });
    } else if (error.status === 400) {
      this.errorMessage = 'Invalid input. Please check your information.';
    } else if (error.status === 0) {
      this.errorMessage = 'Network error. Please check your connection.';
    } else {
      this.errorMessage = error.error?.message || 'Failed to update profile. Please try again.';
    }
    
    this.notificationService.showError(this.errorMessage);
  }

  /**
   * Cancel editing and return to profile view.
   */
  cancel(): void {
    this.router.navigate(['/profile']);
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
    const field = this.editForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Get error message for a specific field.
   */
  getErrorMessage(fieldName: string): string {
    const field = this.editForm.get(fieldName);
    
    if (!field || !field.errors) {
      return '';
    }

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }

    if (field.errors['invalidEmail']) {
      return 'Please enter a valid email address';
    }

    if (field.errors['emailTaken']) {
      return 'This email is already in use';
    }

    if (field.errors['minlength']) {
      const minLength = field.errors['minlength'].requiredLength;
      return `${this.getFieldLabel(fieldName)} must be at least ${minLength} characters`;
    }

    return 'Invalid input';
  }

  /**
   * Get user-friendly label for form field.
   */
  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      email: 'Email',
      firstName: 'First name',
      lastName: 'Last name'
    };
    return labels[fieldName] || fieldName;
  }
}
