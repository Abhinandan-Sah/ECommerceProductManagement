/**
 * API Response Models
 * 
 * These models define the data structures for API responses,
 * error handling, and validation errors.
 */

/**
 * Generic API response wrapper
 */
export interface ApiResponse<T> {
  data?: T;
  message?: string;
  errors?: string[];
}

/**
 * API error response
 */
export interface ApiError {
  message: string;
  statusCode?: number;
  errors?: { [key: string]: string[] };
}

/**
 * Validation error details
 */
export interface ValidationError {
  field: string;
  message: string;
}
