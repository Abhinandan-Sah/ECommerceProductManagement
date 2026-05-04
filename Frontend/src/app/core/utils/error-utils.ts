import { HttpErrorResponse } from '@angular/common/http';

// Converts HTTP errors into messages we can show in the UI.
export function extractErrorMessage(error: HttpErrorResponse): string {
  if (error.status === 400 && error.error?.errors) {
    return formatValidationErrors(error.error.errors);
  }

  if (error.status === 409 && error.error?.message) {
    return error.error.message;
  }

  if (error.status === 401) {
    return 'Your session has expired. Please log in again.';
  }

  if (error.status === 403) {
    return "You don't have permission to access this resource.";
  }

  if (error.status === 404) {
    return 'The requested resource was not found.';
  }

  if (error.status === 500) {
    return 'An unexpected error occurred. Please try again later.';
  }

  if (error.error?.message) {
    return error.error.message;
  }

  return getDefaultErrorMessage(error.status);
}

function formatValidationErrors(errors: { [field: string]: string[] }): string {
  const errorMessages: string[] = [];
  
  for (const field in errors) {
    if (errors.hasOwnProperty(field)) {
      const fieldErrors = errors[field];
      if (Array.isArray(fieldErrors)) {
        errorMessages.push(...fieldErrors);
      }
    }
  }

  return errorMessages.length > 0 
    ? errorMessages.join('. ') 
    : 'Validation failed. Please check your input.';
}

function getDefaultErrorMessage(status: number): string {
  switch (status) {
    case 0:
      return 'Unable to connect to the server. Please check your internet connection.';
    case 400:
      return 'Invalid request. Please check your input.';
    case 401:
      return 'Authentication required. Please log in.';
    case 403:
      return 'Access denied. You do not have permission.';
    case 404:
      return 'Resource not found.';
    case 409:
      return 'Conflict detected. The resource may already exist.';
    case 500:
      return 'Server error. Please try again later.';
    case 503:
      return 'A backend service is unavailable. Please make sure the required API is running.';
    default:
      return `An error occurred (Status: ${status}). Please try again.`;
  }
}
