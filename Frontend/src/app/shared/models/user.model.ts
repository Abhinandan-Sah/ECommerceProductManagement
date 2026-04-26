/**
 * User Models
 * 
 * These models define the data structures for user-related operations
 * including user profiles, roles, and user management.
 */

/**
 * User role enumeration
 */
export enum UserRole {
  User = 'User',
  Admin = 'Admin'
}

/**
 * User entity representing a system user
 */
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

/**
 * User profile information
 */
export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  isActive: boolean;
}

/**
 * Request payload for updating user profile
 */
export interface UpdateProfileRequest {
  email: string;
  firstName: string;
  lastName: string;
}

/**
 * Request payload for updating user role (admin operation)
 */
export interface UpdateRoleRequest {
  role: string;
}

/**
 * Request payload for updating user status (admin operation)
 */
export interface UpdateStatusRequest {
  isActive: boolean;
}

/**
 * Generic paginated response wrapper
 */
export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
