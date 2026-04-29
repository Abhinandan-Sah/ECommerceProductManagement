import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';

/**
 * Service for managing global loading state across the application.
 *
 * Uses a request counter so concurrent HTTP requests keep the overlay
 * visible until ALL of them complete. The counter is clamped to >= 0
 * to prevent desync from unbalanced show/hide calls.
 */
@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private requestCount = 0;

  /**
   * Observable that emits the current loading state (de-duped).
   * Components subscribe to this to show/hide loading indicators.
   */
  public loading$: Observable<boolean> = this.loadingSubject
    .asObservable()
    .pipe(distinctUntilChanged());

  /** Increments the counter and activates the loading overlay. */
  show(): void {
    this.requestCount++;
    this.loadingSubject.next(true);
  }

  /** Decrements the counter; hides the overlay when all requests finish. */
  hide(): void {
    this.requestCount = Math.max(0, this.requestCount - 1);
    if (this.requestCount === 0) {
      this.loadingSubject.next(false);
    }
  }

  /**
   * Hard-reset — call this on navigation events or when recovering
   * from an unexpected error that may have left the counter stuck.
   */
  reset(): void {
    this.requestCount = 0;
    this.loadingSubject.next(false);
  }

  /** Synchronously check current loading state. */
  isLoading(): boolean {
    return this.loadingSubject.value;
  }
}
