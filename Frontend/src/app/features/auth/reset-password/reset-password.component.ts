import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  form!: FormGroup;
  token: string | null = null;
  email: string | null = null;
  isSubmitting = false;
  isSuccess = false;

  ngOnInit(): void {
    // Extract token and email from query parameters
    this.route.queryParams.subscribe(params => {
      this.token = params['token'] || null;
      this.email = params['email'] || null;

      if (!this.token) {
        this.notify.showError('Invalid reset link. Please request a new password reset.');
        this.router.navigate(['/login']);
      }
    });

    this.initForm();
  }

  initForm(): void {
    this.form = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    
    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.form.invalid || !this.token) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const payload = {
      token: this.token,
      newPassword: this.form.value.newPassword,
      confirmPassword: this.form.value.confirmPassword
    };

    this.authService.resetPassword(payload).subscribe({
      next: () => {
        this.isSuccess = true;
        this.notify.showSuccess('Password reset successfully! Redirecting to login...');
        
        // Redirect to login after 3 seconds
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 3000);
      },
      error: (err) => {
        this.isSubmitting = false;
        const errorMessage = err.error?.message || 'Failed to reset password';
        
        if (errorMessage.includes('Invalid or expired')) {
          this.notify.showError('Reset link has expired. Please request a new password reset.');
        } else {
          this.notify.showError(errorMessage);
        }
      }
    });
  }

  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }
}
