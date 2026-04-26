import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  User, Role, UpdateProfileRequest,
  UpdateRoleRequest, UpdateStatusRequest, PaginatedResponse
} from '../../shared/models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private readonly base = '/api/users';

  getProfile(): Observable<User> {
    return this.http.get<User>('/api/auth/profile');
  }

  updateProfile(data: UpdateProfileRequest): Observable<User> {
    return this.http.put<User>(`${this.base}/me`, data);
  }

  getUsers(
    page = 1, pageSize = 10,
    search?: string, role?: string
  ): Observable<PaginatedResponse<User>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    if (role)   params = params.set('role', role);
    return this.http.get<PaginatedResponse<User>>(this.base, { params });
  }

  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.base}/${id}`);
  }

  updateUserRole(id: string, role: string): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/role`, { role });
  }

  updateUserStatus(id: string, isActive: boolean): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { isActive });
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
