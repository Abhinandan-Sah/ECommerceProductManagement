import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProductResponse, CreateProduct, UpdateProduct } from '../models/product.model';
import { ProductVariantResponse, CreateProductVariant, UpdateProductVariant } from '../models/product-variant.model';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private http = inject(HttpClient);
  private baseUrl = '/api/products';

  getProducts(categoryId?: string, status?: number): Observable<ProductResponse[]> {
    let params = new HttpParams();
    if (categoryId) params = params.set('categoryId', categoryId);
    if (status !== undefined) params = params.set('status', status.toString());

    return this.http.get<ProductResponse[]>(this.baseUrl, { params });
  }

  getProductById(id: string): Observable<ProductResponse> {
    return this.http.get<ProductResponse>(`${this.baseUrl}/${id}`);
  }

  createProduct(dto: CreateProduct): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(this.baseUrl, dto);
  }

  updateProduct(id: string, dto: UpdateProduct): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, dto);
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getVariants(productId: string): Observable<ProductVariantResponse[]> {
    return this.http.get<ProductVariantResponse[]>(`${this.baseUrl}/${productId}/variants`);
  }

  getVariantById(productId: string, variantId: string): Observable<ProductVariantResponse> {
    return this.http.get<ProductVariantResponse>(`${this.baseUrl}/${productId}/variants/${variantId}`);
  }

  createVariant(productId: string, dto: CreateProductVariant): Observable<ProductVariantResponse> {
    return this.http.post<ProductVariantResponse>(`${this.baseUrl}/${productId}/variants`, dto);
  }

  updateVariant(productId: string, id: string, dto: UpdateProductVariant): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${productId}/variants/${id}`, dto);
  }

  deleteVariant(productId: string, id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${productId}/variants/${id}`);
  }
}
