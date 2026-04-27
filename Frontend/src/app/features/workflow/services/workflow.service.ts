import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  UpdatePricingRequest,
  UpdateInventoryRequest,
  UpdateStatusRequest,
  WorkflowMessageResponse
} from '../models/workflow.model';

@Injectable({
  providedIn: 'root'
})
export class WorkflowService {
  private http = inject(HttpClient);
  private baseUrl = '/api/workflow/products';

  updatePricing(productId: string, dto: UpdatePricingRequest): Observable<WorkflowMessageResponse> {
    return this.http.put<WorkflowMessageResponse>(`${this.baseUrl}/${productId}/pricing`, dto);
  }

  updateInventory(productId: string, dto: UpdateInventoryRequest): Observable<WorkflowMessageResponse> {
    return this.http.put<WorkflowMessageResponse>(`${this.baseUrl}/${productId}/inventory`, dto);
  }

  submitForReview(productId: string): Observable<WorkflowMessageResponse> {
    return this.http.post<WorkflowMessageResponse>(`${this.baseUrl}/${productId}/submit`, {});
  }

  updateStatus(productId: string, dto: UpdateStatusRequest): Observable<WorkflowMessageResponse> {
    return this.http.put<WorkflowMessageResponse>(`${this.baseUrl}/${productId}/status`, dto);
  }
}
