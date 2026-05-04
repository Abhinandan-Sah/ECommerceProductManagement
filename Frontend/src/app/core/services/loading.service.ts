import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';

// Tracks the global loading overlay.
@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private requestCount = 0;

  public loading$: Observable<boolean> = this.loadingSubject
    .asObservable()
    .pipe(distinctUntilChanged());

  show(): void {
    this.requestCount++;
    this.loadingSubject.next(true);
  }

  hide(): void {
    this.requestCount = Math.max(0, this.requestCount - 1);
    if (this.requestCount === 0) {
      this.loadingSubject.next(false);
    }
  }

  // Clears any stuck loading state.
  reset(): void {
    this.requestCount = 0;
    this.loadingSubject.next(false);
  }

  isLoading(): boolean {
    return this.loadingSubject.value;
  }
}
