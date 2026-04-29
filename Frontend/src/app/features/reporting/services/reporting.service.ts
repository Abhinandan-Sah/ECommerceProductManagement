import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { DashboardKpi, ProductReport, ProductReportFilter, PagedResult } from '../models/reporting.model';
import { NotificationService } from '../../../core/services/notification.service';
import { extractErrorMessage } from '../../../core/utils/error-utils';

@Injectable({
  providedIn: 'root'
})
export class ReportingService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private baseUrl = '/api/reports';

  getDashboardKpi(): Observable<DashboardKpi> {
    return this.http.get<DashboardKpi>(`${this.baseUrl}/dashboard`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getProductReports(filter: ProductReportFilter): Observable<PagedResult<ProductReport>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber.toString())
      .set('pageSize', filter.pageSize.toString());
    
    if (filter.category) {
      params = params.set('category', filter.category);
    }
    if (filter.status) {
      params = params.set('status', filter.status);
    }

    return this.http.get<PagedResult<ProductReport>>(`${this.baseUrl}/products`, { params }).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getHistoricalSnapshots(): Observable<DashboardKpi[]> {
    return this.http.get<DashboardKpi[]>(`${this.baseUrl}/snapshots`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  exportProductsCsv(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/products/export`, {
      responseType: 'blob'
    }).pipe(
      tap(() => this.notificationService.showSuccess('Report exported successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }
}
