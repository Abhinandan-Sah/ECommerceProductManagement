import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MediaAssetResponse, CreateMediaAsset } from '../models/media-asset.model';

@Injectable({
  providedIn: 'root'
})
export class MediaAssetService {
  private http = inject(HttpClient);
  private baseUrl = '/api/products';

  getMediaByProduct(productId: string): Observable<MediaAssetResponse[]> {
    return this.http.get<MediaAssetResponse[]>(`${this.baseUrl}/${productId}/media`);
  }

  getMediaById(productId: string, mediaId: string): Observable<MediaAssetResponse> {
    return this.http.get<MediaAssetResponse>(`${this.baseUrl}/${productId}/media/${mediaId}`);
  }

  createMedia(productId: string, dto: CreateMediaAsset): Observable<MediaAssetResponse> {
    return this.http.post<MediaAssetResponse>(`${this.baseUrl}/${productId}/media`, dto);
  }

  deleteMedia(productId: string, mediaId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${productId}/media/${mediaId}`);
  }
}
