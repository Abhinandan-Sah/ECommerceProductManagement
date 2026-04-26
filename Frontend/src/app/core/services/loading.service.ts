import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Service for managing global loading state across the application.
 * 
 * This service:
 * - Tracks the number of active HTTP requests
 * - Provides an observable for components to subscribe to loading state
 * - Shows loading indicator when any request is active
 * - Hides loading indicator when all requests complete
 * 
 * The service uses a request counter to handle multiple concurrent requests.
 * Loading state is only set to false when all requests have completed.
 */
@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private requestCount = 0;

  /**
   * Observable that emits the current loading state.
   * Components can subscribe to this to show/hide loading indicators.
   */
  public loading$: Observable<boolean> = this.loadingSubject.asObservable();

  /**
   * Increments the request counter and sets loading state to true.
   * Called when an HTTP request starts.
   */
  show(): void {
    this.requestCount++;
    if (this.requestCount === 1) {
      this.loadingSubject.next(true);
    }
  }

  /**
   * Decrements the request counter and sets loading state to false
   * when all requests have completed.
   * Called when an HTTP request completes (success or error).
   */
  hide(): void {
    this.requestCount--;
    if (this.requestCount <= 0) {
      this.requestCount = 0;
      this.loadingSubject.next(false);
    }
  }

  /**
   * Gets the current loading state synchronously.
   * 
   * @returns True if any requests are active, false otherwise
   */
  isLoading(): boolean {
    return this.loadingSubject.value;
  }
}
