import { Component, OnInit, inject } from '@angular/core';

import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { emailValidator } from '../../../shared/utils/validators';
import { User, UpdateProfileRequest } from '../../../shared/models/user.model';

@Component({
  selector: 'app-edit-profile',
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './edit-profile.component.html',
  styleUrls: ['./edit-profile.component.css']
})
export class EditProfileComponent implements OnInit {
  private fb                  = inject(FormBuilder);
  private userService         = inject(UserService);
  private notificationService = inject(NotificationService);
  private router              = inject(Router);

  editForm!: FormGroup;
  isLoading = true;
  isSaving = false;
  errorMessage = '';

  ngOnInit(): void {
    this.initializeForm();
    this.loadProfile();
  }

  private initializeForm(): void {
    this.editForm = this.fb.group({
      email:    ['', [Validators.required, emailValidator()]],
      fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]]
    });
  }

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

  private populateForm(user: User): void {
    this.editForm.patchValue({
      email:    user.email,
      fullName: user.fullName
    });
  }

  onSubmit(): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const updateData: UpdateProfileRequest = {
      email:    this.editForm.value.email,
      fullName: this.editForm.value.fullName
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

  cancel(): void {
    this.router.navigate(['/profile']);
  }

  hasError(fieldName: string): boolean {
    const field = this.editForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

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

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      email:    'Email',
      fullName: 'Full name'
    };
    return labels[fieldName] || fieldName;
  }
}
