import { HttpErrorResponse } from '@angular/common/http';

/**
 * Extracts a user-friendly error message from an HTTP error response.
 * Handles different error formats and status codes.
 */
export function extractErrorMessage(error: HttpErrorResponse): string {
  // Handle 400 Bad Request with validation errors
  if (error.status === 400 && error.error?.errors) {
    return formatValidationErrors(error.error.errors);
  }

  // Handle 409 Conflict
  if (error.status === 409 && error.error?.message) {
    return error.error.message;
  }

  // Handle 401 Unauthorized
  if (error.status === 401) {
    return 'Your session has expired. Please log in again.';
  }

  // Handle 403 Forbidden
  if (error.status === 403) {
    return "You don't have permission to access this resource.";
  }

  // Handle 404 Not Found
  if (error.status === 404) {
    return 'The requested resource was not found.';
  }

  // Handle 500 Internal Server Error
  if (error.status === 500) {
    return 'An unexpected error occurred. Please try again later.';
  }

  // Handle standard error response with message
  if (error.error?.message) {
    return error.error.message;
  }

  // Fallback to generic message
  return getDefaultErrorMessage(error.status);
}

/**
 * Formats validation errors from a 400 Bad Request response.
 */
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

/**
 * Returns a default error message based on HTTP status code.
 */
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
      return 'Service temporarily unavailable. Please try again later.';
    default:
      return `An error occurred (Status: ${status}). Please try again.`;
  }
}
