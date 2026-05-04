import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-validation-message',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="form-error" *ngIf="shouldShowError()">
      {{ getErrorMessage() }}
    </div>
  `,
  styles: [`
    .form-error {
      display: block;
      margin-top: var(--spacing-xs);
      font-size: var(--font-size-sm);
      color: var(--error-color);
    }
  `]
})
export class ValidationMessageComponent {
  @Input() control: AbstractControl | null = null;
  @Input() fieldName: string = 'This field';
  @Input() customMessages: { [key: string]: string } = {};

  shouldShowError(): boolean {
    return !!(this.control && this.control.invalid && (this.control.dirty || this.control.touched));
  }

  getErrorMessage(): string {
    if (!this.control || !this.control.errors) {
      return '';
    }

    const errors = this.control.errors;

    for (const errorKey in errors) {
      if (this.customMessages[errorKey]) {
        return this.customMessages[errorKey];
      }
    }

    if (errors['required']) {
      return `${this.fieldName} is required.`;
    }

    if (errors['email']) {
      return 'Please enter a valid email address.';
    }

    if (errors['minlength']) {
      const minLength = errors['minlength'].requiredLength;
      return `${this.fieldName} must be at least ${minLength} characters long.`;
    }

    if (errors['maxlength']) {
      const maxLength = errors['maxlength'].requiredLength;
      return `${this.fieldName} must not exceed ${maxLength} characters.`;
    }

    if (errors['pattern']) {
      return `${this.fieldName} format is invalid.`;
    }

    if (errors['passwordMismatch']) {
      return 'Passwords do not match.';
    }

    if (errors['passwordStrength']) {
      return 'Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.';
    }

    if (errors['min']) {
      const min = errors['min'].min;
      return `${this.fieldName} must be at least ${min}.`;
    }

    if (errors['max']) {
      const max = errors['max'].max;
      return `${this.fieldName} must not exceed ${max}.`;
    }

    return `${this.fieldName} is invalid.`;
  }
}
