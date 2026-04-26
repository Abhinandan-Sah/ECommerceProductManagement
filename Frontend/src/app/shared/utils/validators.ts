import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Custom validators for form validation.
 * 
 * These validators provide additional validation logic beyond Angular's built-in validators,
 * specifically for email format, password strength, and password confirmation matching.
 * 
 * Requirements: 1.5, 1.6, 6.5, 6.6
 */

/**
 * Validates email format.
 * 
 * Checks that the email contains an @ symbol, has a valid domain structure,
 * and doesn't contain invalid characters.
 * 
 * @returns A validator function that returns null if valid, or an error object if invalid
 * 
 * Validates: Requirements 1.5
 */
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

/**
 * Validates password strength.
 * 
 * Ensures the password meets minimum security requirements:
 * - At least 8 characters long
 * - Contains at least one uppercase letter
 * - Contains at least one lowercase letter
 * - Contains at least one digit
 * - Contains at least one special character
 * 
 * @returns A validator function that returns null if valid, or an error object with specific requirements that failed
 * 
 * Validates: Requirements 1.6, 6.6
 */
export function passwordStrengthValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null; // Don't validate empty values
    }

    const password = control.value;
    const errors: ValidationErrors = {};

    // Minimum length check
    if (password.length < 8) {
      errors['minLength'] = true;
    }

    // Uppercase letter check
    if (!/[A-Z]/.test(password)) {
      errors['requiresUppercase'] = true;
    }

    // Lowercase letter check
    if (!/[a-z]/.test(password)) {
      errors['requiresLowercase'] = true;
    }

    // Digit check
    if (!/\d/.test(password)) {
      errors['requiresDigit'] = true;
    }

    // Special character check
    if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
      errors['requiresSpecialChar'] = true;
    }

    return Object.keys(errors).length > 0 ? { passwordStrength: errors } : null;
  };
}

/**
 * Validates that password confirmation matches the password field.
 * 
 * This validator should be applied to the form group, not individual controls,
 * as it needs to compare two fields.
 * 
 * @param passwordField - The name of the password field
 * @param confirmPasswordField - The name of the confirm password field
 * @returns A validator function that returns null if passwords match, or an error object if they don't
 * 
 * Validates: Requirements 6.5, 6.6
 */
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

    // Set error on the confirmPassword field
    if (!passwordsMatch) {
      confirmPassword.setErrors({ ...confirmPassword.errors, passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      // Clear passwordMismatch error if passwords match
      if (confirmPassword.errors) {
        const errors = { ...confirmPassword.errors };
        delete errors['passwordMismatch'];
        confirmPassword.setErrors(Object.keys(errors).length > 0 ? errors : null);
      }
    }

    return null;
  };
}

/**
 * Helper function to get user-friendly error messages for validation errors.
 * 
 * @param errors - The validation errors object from a form control
 * @returns An array of user-friendly error messages
 */
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
