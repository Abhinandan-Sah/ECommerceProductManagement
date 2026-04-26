import { FormControl, FormGroup } from '@angular/forms';
import {
  emailValidator,
  passwordStrengthValidator,
  passwordMatchValidator,
  getValidationErrorMessages
} from './validators';

describe('Validators', () => {
  describe('emailValidator', () => {
    it('should return null for valid email addresses', () => {
      const validator = emailValidator();
      const validEmails = [
        'test@example.com',
        'user.name@example.com',
        'user+tag@example.co.uk',
        'user_name@example-domain.com',
        'user123@test.org'
      ];

      validEmails.forEach(email => {
        const control = new FormControl(email);
        expect(validator(control)).toBeNull();
      });
    });

    it('should return error for invalid email addresses', () => {
      const validator = emailValidator();
      const invalidEmails = [
        'invalid',
        'invalid@',
        '@example.com',
        'invalid@.com',
        'invalid@domain',
        'invalid @example.com',
        'invalid@exam ple.com'
      ];

      invalidEmails.forEach(email => {
        const control = new FormControl(email);
        const result = validator(control);
        expect(result).not.toBeNull();
        expect(result?.['invalidEmail']).toBeDefined();
      });
    });

    it('should return null for empty value', () => {
      const validator = emailValidator();
      const control = new FormControl('');
      expect(validator(control)).toBeNull();
    });

    it('should return null for null value', () => {
      const validator = emailValidator();
      const control = new FormControl(null);
      expect(validator(control)).toBeNull();
    });
  });

  describe('passwordStrengthValidator', () => {
    it('should return null for strong passwords', () => {
      const validator = passwordStrengthValidator();
      const strongPasswords = [
        'Password123!',
        'MyP@ssw0rd',
        'Str0ng!Pass',
        'C0mpl3x@Pass',
        'Secur3#Password'
      ];

      strongPasswords.forEach(password => {
        const control = new FormControl(password);
        expect(validator(control)).toBeNull();
      });
    });

    it('should return error for password shorter than 8 characters', () => {
      const validator = passwordStrengthValidator();
      const control = new FormControl('Pass1!');
      const result = validator(control);
      
      expect(result).not.toBeNull();
      expect(result?.['passwordStrength']?.['minLength']).toBe(true);
    });

    it('should return error for password without uppercase letter', () => {
      const validator = passwordStrengthValidator();
      const control = new FormControl('password123!');
      const result = validator(control);
      
      expect(result).not.toBeNull();
      expect(result?.['passwordStrength']?.['requiresUppercase']).toBe(true);
    });

    it('should return error for password without lowercase letter', () => {
      const validator = passwordStrengthValidator();
      const control = new FormControl('PASSWORD123!');
      const result = validator(control);
      
      expect(result).not.toBeNull();
      expect(result?.['passwordStrength']?.['requiresLowercase']).toBe(true);
    });

    it('should return error for password without digit', () => {
      const validator = passwordStrengthValidator();
      const control = new FormControl('Password!');
      const result = validator(control);
      
      expect(result).not.toBeNull();
      expect(result?.['passwordStrength']?.['requiresDigit']).toBe(true);
    });

    it('should return error for password without special character', () => {
      const validator = passwordStrengthValidator();
      const control = new FormControl('Password123');
      const result = validator(control);
      
      expect(result).not.toBeNull();
      expect(result?.['passwordStrength']?.['requiresSpecialChar']).toBe(true);
    });

    it('should return multiple errors for weak password', () => {
      const validator = passwordStrengthValidator();
      const control = new FormControl('weak');
      const result = validator(control);
      
      expect(result).not.toBeNull();
      expect(result?.['passwordStrength']?.['minLength']).toBe(true);
      expect(result?.['passwordStrength']?.['requiresUppercase']).toBe(true);
      expect(result?.['passwordStrength']?.['requiresDigit']).toBe(true);
      expect(result?.['passwordStrength']?.['requiresSpecialChar']).toBe(true);
    });

    it('should return null for empty value', () => {
      const validator = passwordStrengthValidator();
      const control = new FormControl('');
      expect(validator(control)).toBeNull();
    });

    it('should return null for null value', () => {
      const validator = passwordStrengthValidator();
      const control = new FormControl(null);
      expect(validator(control)).toBeNull();
    });
  });

  describe('passwordMatchValidator', () => {
    it('should return null when passwords match', () => {
      const form = new FormGroup({
        password: new FormControl('Password123!'),
        confirmPassword: new FormControl('Password123!')
      });

      const validator = passwordMatchValidator('password', 'confirmPassword');
      const result = validator(form);

      expect(result).toBeNull();
      expect(form.get('confirmPassword')?.errors).toBeNull();
    });

    it('should return error when passwords do not match', () => {
      const form = new FormGroup({
        password: new FormControl('Password123!'),
        confirmPassword: new FormControl('DifferentPass123!')
      });

      const validator = passwordMatchValidator('password', 'confirmPassword');
      const result = validator(form);

      expect(result).not.toBeNull();
      expect(result?.['passwordMismatch']).toBe(true);
      expect(form.get('confirmPassword')?.errors?.['passwordMismatch']).toBe(true);
    });

    it('should return null when confirmPassword is empty', () => {
      const form = new FormGroup({
        password: new FormControl('Password123!'),
        confirmPassword: new FormControl('')
      });

      const validator = passwordMatchValidator('password', 'confirmPassword');
      const result = validator(form);

      expect(result).toBeNull();
    });

    it('should return null when password field does not exist', () => {
      const form = new FormGroup({
        confirmPassword: new FormControl('Password123!')
      });

      const validator = passwordMatchValidator('password', 'confirmPassword');
      const result = validator(form);

      expect(result).toBeNull();
    });

    it('should return null when confirmPassword field does not exist', () => {
      const form = new FormGroup({
        password: new FormControl('Password123!')
      });

      const validator = passwordMatchValidator('password', 'confirmPassword');
      const result = validator(form);

      expect(result).toBeNull();
    });

    it('should clear passwordMismatch error when passwords match after mismatch', () => {
      const form = new FormGroup({
        password: new FormControl('Password123!'),
        confirmPassword: new FormControl('DifferentPass123!')
      });

      const validator = passwordMatchValidator('password', 'confirmPassword');
      
      // First validation - passwords don't match
      validator(form);
      expect(form.get('confirmPassword')?.errors?.['passwordMismatch']).toBe(true);

      // Update confirmPassword to match
      form.get('confirmPassword')?.setValue('Password123!');
      validator(form);
      
      expect(form.get('confirmPassword')?.errors).toBeNull();
    });

    it('should preserve other errors when clearing passwordMismatch error', () => {
      const form = new FormGroup({
        password: new FormControl('Password123!'),
        confirmPassword: new FormControl('DifferentPass123!')
      });

      const validator = passwordMatchValidator('password', 'confirmPassword');
      
      // First validation - passwords don't match
      validator(form);
      
      const confirmPasswordControl = form.get('confirmPassword');
      
      // Manually add another error after the mismatch error is set
      const existingErrors = confirmPasswordControl?.errors || {};
      confirmPasswordControl?.setErrors({ ...existingErrors, customError: true });
      
      expect(confirmPasswordControl?.errors?.['passwordMismatch']).toBe(true);
      expect(confirmPasswordControl?.errors?.['customError']).toBe(true);

      // Update password to match confirmPassword (instead of the other way around)
      form.get('password')?.setValue('DifferentPass123!');
      validator(form);
      
      // passwordMismatch should be cleared, but customError should remain
      expect(confirmPasswordControl?.errors?.['passwordMismatch']).toBeUndefined();
      expect(confirmPasswordControl?.errors?.['customError']).toBe(true);
    });
  });

  describe('getValidationErrorMessages', () => {
    it('should return empty array for null errors', () => {
      const messages = getValidationErrorMessages(null);
      expect(messages).toEqual([]);
    });

    it('should return message for required error', () => {
      const errors = { required: true };
      const messages = getValidationErrorMessages(errors);
      expect(messages).toContain('This field is required');
    });

    it('should return message for invalidEmail error', () => {
      const errors = { invalidEmail: { value: 'invalid' } };
      const messages = getValidationErrorMessages(errors);
      expect(messages).toContain('Please enter a valid email address');
    });

    it('should return messages for passwordStrength errors', () => {
      const errors = {
        passwordStrength: {
          minLength: true,
          requiresUppercase: true,
          requiresLowercase: true,
          requiresDigit: true,
          requiresSpecialChar: true
        }
      };
      const messages = getValidationErrorMessages(errors);
      
      expect(messages).toContain('Password must be at least 8 characters long');
      expect(messages).toContain('Password must contain at least one uppercase letter');
      expect(messages).toContain('Password must contain at least one lowercase letter');
      expect(messages).toContain('Password must contain at least one digit');
      expect(messages).toContain('Password must contain at least one special character');
    });

    it('should return message for passwordMismatch error', () => {
      const errors = { passwordMismatch: true };
      const messages = getValidationErrorMessages(errors);
      expect(messages).toContain('Passwords do not match');
    });

    it('should return message for minlength error', () => {
      const errors = { minlength: { requiredLength: 10, actualLength: 5 } };
      const messages = getValidationErrorMessages(errors);
      expect(messages).toContain('Minimum length is 10 characters');
    });

    it('should return message for maxlength error', () => {
      const errors = { maxlength: { requiredLength: 50, actualLength: 60 } };
      const messages = getValidationErrorMessages(errors);
      expect(messages).toContain('Maximum length is 50 characters');
    });

    it('should return multiple messages for multiple errors', () => {
      const errors = {
        required: true,
        invalidEmail: { value: 'invalid' }
      };
      const messages = getValidationErrorMessages(errors);
      
      expect(messages.length).toBe(2);
      expect(messages).toContain('This field is required');
      expect(messages).toContain('Please enter a valid email address');
    });
  });
});
