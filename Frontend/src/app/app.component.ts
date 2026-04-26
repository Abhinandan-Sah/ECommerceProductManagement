import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { NavigationComponent } from './shared/components/navigation/navigation.component';
import { LoadingSpinnerComponent } from './shared/components/loading-spinner/loading-spinner.component';
import { initAuth } from './store/auth/auth.actions';

@Component({
    selector: 'app-root',
    imports: [RouterOutlet, NavigationComponent, LoadingSpinnerComponent],
    templateUrl: './app.component.html',
    styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private store = inject(Store);
  title = 'identity-ui';

  ngOnInit(): void {
    // Attempt to restore session from persisted refresh token on every page load
    this.store.dispatch(initAuth());
  }
}
