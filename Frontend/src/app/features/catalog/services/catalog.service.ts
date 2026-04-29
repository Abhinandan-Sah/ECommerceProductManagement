import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { ProductResponse, CreateProduct, UpdateProduct } from '../models/product.model';
import { ProductVariantResponse, CreateProductVariant, UpdateProductVariant } from '../models/product-variant.model';
import { NotificationService } from '../../../core/services/notification.service';
import { extractErrorMessage } from '../../../core/utils/error-utils';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private baseUrl = '/api/products';

  getProducts(categoryId?: string, status?: number): Observable<ProductResponse[]> {
    let params = new HttpParams();
    if (categoryId) params = params.set('categoryId', categoryId);
    if (status !== undefined) params = params.set('status', status.toString());

    return this.http.get<ProductResponse[]>(this.baseUrl, { params }).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getProductById(id: string): Observable<ProductResponse> {
    return this.http.get<ProductResponse>(`${this.baseUrl}/${id}`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  createProduct(dto: CreateProduct): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(this.baseUrl, dto).pipe(
      tap(() => this.notificationService.showSuccess('Product created successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  updateProduct(id: string, dto: UpdateProduct): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto).pipe(
      tap(() => this.notificationService.showSuccess('Product updated successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.notificationService.showSuccess('Product deleted successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getVariants(productId: string): Observable<ProductVariantResponse[]> {
    return this.http.get<ProductVariantResponse[]>(`${this.baseUrl}/${productId}/variants`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getVariantById(productId: string, variantId: string): Observable<ProductVariantResponse> {
    return this.http.get<ProductVariantResponse>(`${this.baseUrl}/${productId}/variants/${variantId}`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  createVariant(productId: string, dto: CreateProductVariant): Observable<ProductVariantResponse> {
    return this.http.post<ProductVariantResponse>(`${this.baseUrl}/${productId}/variants`, dto).pipe(
      tap(() => this.notificationService.showSuccess('Product variant created successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  updateVariant(productId: string, id: string, dto: UpdateProductVariant): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${productId}/variants/${id}`, dto).pipe(
      tap(() => this.notificationService.showSuccess('Product variant updated successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  deleteVariant(productId: string, id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${productId}/variants/${id}`).pipe(
      tap(() => this.notificationService.showSuccess('Product variant deleted successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }
}
