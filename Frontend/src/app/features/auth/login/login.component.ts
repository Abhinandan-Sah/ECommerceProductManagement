import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { emailValidator } from '../../../shared/utils/validators';
import { AuthStateService } from '../../../core/state/auth-state.service';


@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  private auth = inject(AuthStateService);
  private fb = inject(FormBuilder);

  loading = this.auth.loading;
  error = this.auth.error;

  form = this.fb.group({
    email:    ['', [Validators.required, emailValidator()]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.auth.clearError();
    this.auth.login(this.form.value.email!, this.form.value.password!).subscribe({
      error: () => {}
    });
  }

  isInvalid(field: string): boolean {
    const c = this.form.get(field);
    return !!(c?.invalid && (c.dirty || c.touched));
  }

  getError(field: string): string {
    const c = this.form.get(field);
    if (!c?.errors) return '';
    if (c.errors['required'])     return `${field} is required`;
    if (c.errors['invalidEmail']) return 'Enter a valid email address';
    if (c.errors['minlength'])    return 'Password must be at least 6 characters';
    return '';
  }
}
