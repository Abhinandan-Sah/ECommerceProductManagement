import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { CategoryResponse, CreateCategory, UpdateCategory } from '../models/category.model';
import { NotificationService } from '../../../core/services/notification.service';
import { extractErrorMessage } from '../../../core/utils/error-utils';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private baseUrl = '/api/categories';

  getCategories(): Observable<CategoryResponse[]> {
    return this.http.get<CategoryResponse[]>(this.baseUrl).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getCategoryById(id: string): Observable<CategoryResponse> {
    return this.http.get<CategoryResponse>(`${this.baseUrl}/${id}`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  createCategory(dto: CreateCategory): Observable<CategoryResponse> {
    return this.http.post<CategoryResponse>(this.baseUrl, dto).pipe(
      tap(() => this.notificationService.showSuccess('Category created successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  updateCategory(id: string, dto: UpdateCategory): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto).pipe(
      tap(() => this.notificationService.showSuccess('Category updated successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.notificationService.showSuccess('Category deleted successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }
}
