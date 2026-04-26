import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  User,
  UserRole,
  UpdateProfileRequest,
  UpdateRoleRequest,
  UpdateStatusRequest,
  PaginatedResponse
} from '../../shared/models/user.model';

/**
 * Service for managing user-related operations.
 * 
 * Responsibilities:
 * - User profile management (view, update)
 * - Admin user management (list, update role, update status, delete)
 * - Pagination and filtering for user lists
 * 
 * All methods return observables for reactive programming.
 * Errors are handled by the ErrorInterceptor.
 */
@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/api/users`;

  constructor(private http: HttpClient) {}

  /**
   * Get current user's profile information.
   * Uses the /api/auth/profile endpoint for authenticated user.
   * 
   * @returns Observable containing user profile data
   */
  getProfile(): Observable<User> {
    return this.http
      .get<ApiUser>(`${environment.apiUrl}/api/auth/profile`)
      .pipe(map(user => this.mapApiUser(user)));
  }

  /**
   * Update current user's profile information.
   * 
   * @param data - Updated profile data (email, firstName, lastName)
   * @returns Observable containing updated user data
   */
  updateProfile(data: UpdateProfileRequest): Observable<User> {
    const fullName = `${data.firstName} ${data.lastName}`.trim();
    return this.http
      .put<ApiUser>(`${this.apiUrl}/me`, {
        email: data.email,
        fullName
      })
      .pipe(map(user => this.mapApiUser(user)));
  }

  /**
   * Get paginated list of all users (admin operation).
   * Supports pagination, search, and role filtering.
   * 
   * @param page - Page number (1-based)
   * @param pageSize - Number of items per page
   * @param search - Optional search term for email or name
   * @param role - Optional role filter
   * @returns Observable containing paginated user list
   */
  getUsers(
    page: number = 1,
    pageSize: number = 10,
    search?: string,
    role?: string
  ): Observable<PaginatedResponse<User>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    if (role) {
      params = params.set('role', role);
    }

    return this.http
      .get<ApiPaginatedResponse<ApiUser>>(this.apiUrl, { params })
      .pipe(
        map(response => ({
          ...response,
          data: response.data.map(user => this.mapApiUser(user))
        }))
      );
  }

  /**
   * Get a specific user by ID (admin operation).
   * 
   * @param id - User ID
   * @returns Observable containing user data
   */
  getUserById(id: string): Observable<User> {
    return this.http
      .get<ApiUser>(`${this.apiUrl}/${id}`)
      .pipe(map(user => this.mapApiUser(user)));
  }

  /**
   * Update a user's role (admin operation).
   * 
   * @param id - User ID
   * @param role - New role value
   * @returns Observable that completes on successful update
   */
  updateUserRole(id: string, role: string): Observable<void> {
    const request: UpdateRoleRequest = { role };
    return this.http.put<void>(`${this.apiUrl}/${id}/role`, request);
  }

  /**
   * Update a user's active status (admin operation).
   * 
   * @param id - User ID
   * @param isActive - New active status
   * @returns Observable that completes on successful update
   */
  updateUserStatus(id: string, isActive: boolean): Observable<void> {
    const request: UpdateStatusRequest = { isActive };
    return this.http.put<void>(`${this.apiUrl}/${id}/status`, request);
  }

  /**
   * Delete a user account (admin operation).
   * 
   * @param id - User ID
   * @returns Observable that completes on successful deletion
   */
  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Normalize backend user DTO to frontend User shape.
   */
  private mapApiUser(apiUser: ApiUser): User {
    const { firstName, lastName } = this.splitFullName(apiUser.fullName);

    return {
      id: apiUser.id,
      email: apiUser.email,
      firstName,
      lastName,
      role: apiUser.role === 'Admin' ? UserRole.Admin : UserRole.User,
      isActive: apiUser.isActive,
      createdAt: apiUser.createdAt,
      updatedAt: apiUser.updatedAt ?? ''
    };
  }

  /**
   * Convert backend fullName to first/last names expected by UI.
   */
  private splitFullName(fullName: string): { firstName: string; lastName: string } {
    const normalized = (fullName ?? '').trim();
    if (!normalized) {
      return { firstName: '', lastName: '' };
    }

    const parts = normalized.split(/\s+/);
    if (parts.length === 1) {
      return { firstName: parts[0], lastName: '' };
    }

    return {
      firstName: parts[0],
      lastName: parts.slice(1).join(' ')
    };
  }
}

interface ApiUser {
  id: string;
  fullName: string;
  email: string;
  role: 'User' | 'Admin';
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

interface ApiPaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
