import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import {
  UpdatePricingRequest,
  UpdateInventoryRequest,
  UpdateStatusRequest,
  WorkflowMessageResponse,
  ApprovalStatusResponse
} from '../models/workflow.model';
import { NotificationService } from '../../../core/services/notification.service';
import { extractErrorMessage } from '../../../core/utils/error-utils';

@Injectable({
  providedIn: 'root'
})
export class WorkflowService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private baseUrl = '/api/workflow/products';

  getPricing(productId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${productId}/pricing`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  updatePricing(productId: string, dto: UpdatePricingRequest): Observable<WorkflowMessageResponse> {
    return this.http.put<WorkflowMessageResponse>(`${this.baseUrl}/${productId}/pricing`, dto).pipe(
      tap((response) => this.notificationService.showSuccess(response.message || 'Pricing updated successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  updateInventory(productId: string, dto: UpdateInventoryRequest): Observable<WorkflowMessageResponse> {
    return this.http.put<WorkflowMessageResponse>(`${this.baseUrl}/${productId}/inventory`, dto).pipe(
      tap((response) => this.notificationService.showSuccess(response.message || 'Inventory updated successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  submitForReview(productId: string): Observable<WorkflowMessageResponse> {
    return this.http.post<WorkflowMessageResponse>(`${this.baseUrl}/${productId}/submit`, {}).pipe(
      tap((response) => this.notificationService.showSuccess(response.message || 'Product submitted for review')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  updateStatus(productId: string, dto: UpdateStatusRequest): Observable<WorkflowMessageResponse> {
    return this.http.put<WorkflowMessageResponse>(`${this.baseUrl}/${productId}/status`, dto).pipe(
      tap((response) => this.notificationService.showSuccess(response.message || 'Status updated successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getApprovalStatus(productId: string): Observable<ApprovalStatusResponse> {
    return this.http.get<ApprovalStatusResponse>(`${this.baseUrl}/${productId}/status`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getInventory(productId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${productId}/inventory`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getPendingApprovals(): Observable<ApprovalStatusResponse[]> {
    return this.http.get<ApprovalStatusResponse[]>(`${this.baseUrl}/../approvals/pending`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }
}
