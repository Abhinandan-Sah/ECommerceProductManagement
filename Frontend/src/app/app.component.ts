import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
import { Store } from '@ngrx/store';
import { filter } from 'rxjs/operators';
import { NavigationComponent } from './shared/components/navigation/navigation.component';
import { LoadingSpinnerComponent } from './shared/components/loading-spinner/loading-spinner.component';
import { LoadingService } from './core/services/loading.service';
import { initAuth } from './store/auth/auth.actions';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavigationComponent, LoadingSpinnerComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private store        = inject(Store);
  private router       = inject(Router);
  private loadingService = inject(LoadingService);

  title = 'identity-ui';

  ngOnInit(): void {
    // Restore session from persisted refresh token on every page load
    this.store.dispatch(initAuth());

    // Safety net: reset the loading counter on every completed navigation.
    // This prevents the spinner from getting permanently stuck if a request
    // throws an error that bypasses the finalize() in the interceptor.
    this.router.events.pipe(
      filter(e =>
        e instanceof NavigationEnd ||
        e instanceof NavigationCancel ||
        e instanceof NavigationError
      )
    ).subscribe(() => this.loadingService.reset());
  }
}
