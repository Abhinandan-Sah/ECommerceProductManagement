import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardKpi, ProductReport, ProductReportFilter, PagedResult } from '../models/reporting.model';

@Injectable({
  providedIn: 'root'
})
export class ReportingService {
  private http = inject(HttpClient);
  private baseUrl = '/api/reports';

  getDashboardKpi(): Observable<DashboardKpi> {
    return this.http.get<DashboardKpi>(`${this.baseUrl}/dashboard`);
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

    return this.http.get<PagedResult<ProductReport>>(`${this.baseUrl}/products`, { params });
  }

  getHistoricalSnapshots(): Observable<DashboardKpi[]> {
    return this.http.get<DashboardKpi[]>(`${this.baseUrl}/snapshots`);
  }

  exportProductsCsv(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/products/export`, {
      responseType: 'blob'
    });
  }
}
