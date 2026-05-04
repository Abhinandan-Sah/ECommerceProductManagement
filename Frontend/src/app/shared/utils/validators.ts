import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function emailValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values (use required validator for that)
    }

    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    const valid = emailRegex.test(control.value);

    return valid ? null : { invalidEmail: { value: control.value } };
  };
}

// Checks the password rules used by registration and password reset.
export function passwordStrengthValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values
    }

    const password = control.value;
    const errors: ValidationErrors = {};

    if (password.length < 8) {
      errors['minLength'] = true;
    }

    if (!/[A-Z]/.test(password)) {
      errors['requiresUppercase'] = true;
    }

    if (!/[a-z]/.test(password)) {
      errors['requiresLowercase'] = true;
    }

    if (!/\d/.test(password)) {
      errors['requiresDigit'] = true;
    }

    if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
      errors['requiresSpecialChar'] = true;
    }

    return Object.keys(errors).length > 0 ? { passwordStrength: errors } : null;
  };
}

// Form-level validator for matching password fields
export function passwordMatchValidator(
  passwordField: string,
  confirmPasswordField: string
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.get(passwordField);
    const confirmPassword = control.get(confirmPasswordField);

    if (!password || !confirmPassword) {
      return null;
    }

    if (!confirmPassword.value) {
      return null; // Don't validate empty confirm password
    }

    const passwordsMatch = password.value === confirmPassword.value;

    if (!passwordsMatch) {
      confirmPassword.setErrors({ ...confirmPassword.errors, passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      if (confirmPassword.errors) {
        const errors = { ...confirmPassword.errors };
        delete errors['passwordMismatch'];
        confirmPassword.setErrors(Object.keys(errors).length > 0 ? errors : null);
      }
    }

    return null;
  };
}

export function getValidationErrorMessages(errors: ValidationErrors | null): string[] {
  if (!errors) {
    return [];
  }

  const messages: string[] = [];

  if (errors['required']) {
    messages.push('This field is required');
  }

  if (errors['invalidEmail']) {
    messages.push('Please enter a valid email address');
  }

  if (errors['passwordStrength']) {
    const strengthErrors = errors['passwordStrength'];
    if (strengthErrors['minLength']) {
      messages.push('Password must be at least 8 characters long');
    }
    if (strengthErrors['requiresUppercase']) {
      messages.push('Password must contain at least one uppercase letter');
    }
    if (strengthErrors['requiresLowercase']) {
      messages.push('Password must contain at least one lowercase letter');
    }
    if (strengthErrors['requiresDigit']) {
      messages.push('Password must contain at least one digit');
    }
    if (strengthErrors['requiresSpecialChar']) {
      messages.push('Password must contain at least one special character');
    }
  }

  if (errors['passwordMismatch']) {
    messages.push('Passwords do not match');
  }

  if (errors['minlength']) {
    messages.push(`Minimum length is ${errors['minlength'].requiredLength} characters`);
  }

  if (errors['maxlength']) {
    messages.push(`Maximum length is ${errors['maxlength'].requiredLength} characters`);
  }

  return messages;
}
