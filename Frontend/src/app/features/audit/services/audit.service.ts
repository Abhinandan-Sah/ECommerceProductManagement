import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuditLog } from '../models/audit.model';
import { PagedResult } from '../../reporting/models/reporting.model';

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private http = inject(HttpClient);
  private baseUrl = '/api/audit';

  getAllAuditLogs(pageNumber: number, pageSize: number): Observable<PagedResult<AuditLog>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<AuditLog>>(this.baseUrl, { params });
  }

  getProductAuditLogs(productId: string, pageNumber: number, pageSize: number): Observable<PagedResult<AuditLog>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<AuditLog>>(`${this.baseUrl}/product/${productId}`, { params });
  }

  getUserAuditLogs(userId: string, pageNumber: number, pageSize: number): Observable<PagedResult<AuditLog>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    
    return this.http.get<PagedResult<AuditLog>>(`${this.baseUrl}/user/${userId}`, { params });
  }
}
