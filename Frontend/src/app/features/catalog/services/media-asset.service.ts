import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { MediaAssetResponse, CreateMediaAsset } from '../models/media-asset.model';
import { NotificationService } from '../../../core/services/notification.service';
import { extractErrorMessage } from '../../../core/utils/error-utils';

@Injectable({
  providedIn: 'root'
})
export class MediaAssetService {
  private http = inject(HttpClient);
  private notificationService = inject(NotificationService);
  private baseUrl = '/api/products';

  getMediaByProduct(productId: string): Observable<MediaAssetResponse[]> {
    return this.http.get<MediaAssetResponse[]>(`${this.baseUrl}/${productId}/media`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  getMediaById(productId: string, mediaId: string): Observable<MediaAssetResponse> {
    return this.http.get<MediaAssetResponse>(`${this.baseUrl}/${productId}/media/${mediaId}`).pipe(
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  createMedia(productId: string, dto: CreateMediaAsset): Observable<MediaAssetResponse> {
    return this.http.post<MediaAssetResponse>(`${this.baseUrl}/${productId}/media`, dto).pipe(
      tap(() => this.notificationService.showSuccess('Media asset created successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }

  deleteMedia(productId: string, mediaId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${productId}/media/${mediaId}`).pipe(
      tap(() => this.notificationService.showSuccess('Media asset deleted successfully')),
      catchError((error: HttpErrorResponse) => {
        const message = extractErrorMessage(error);
        this.notificationService.showError(message);
        return throwError(() => error);
      })
    );
  }
}
