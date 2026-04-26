import { Component, inject } from '@angular/core';
import { CommonModule, AsyncPipe } from '@angular/common';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { emailValidator } from '../../../shared/utils/validators';
import { login, clearError } from '../../../store/auth/auth.actions';
import {
  selectAuthLoading, selectAuthError
} from '../../../store/auth/auth.selectors';

/**
 * LoginComponent provides the user interface for authentication.
 *
 * Features:
 * - Reactive form with email and password validation
 * - Loading state via NgRx store selector (no local isLoading flag)
 * - Error handling via NgRx store selector (no local errorMessage)
 * - Dispatches login action — navigation handled in AuthEffects
 *
 * Requirements: 2.1, 2.2, 2.4, 2.5, 14.4, 15.2
 */
@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterModule, AsyncPipe],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  private store = inject(Store);
  private fb    = inject(FormBuilder);

  loading$ = this.store.select(selectAuthLoading);
  error$   = this.store.select(selectAuthError);

  form = this.fb.group({
    email:    ['', [Validators.required, emailValidator()]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.store.dispatch(clearError());
    this.store.dispatch(login({
      email:    this.form.value.email!,
      password: this.form.value.password!
    }));
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
