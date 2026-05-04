import { Component, effect, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStateService } from '../../../core/state/auth-state.service';

@Component({
  selector: 'app-dashboard-redirect',
  standalone: true,
  template: '<div class="page-container"><p>Loading dashboard...</p></div>'
})
export class DashboardRedirectComponent {
  private router = inject(Router);
  private auth = inject(AuthStateService);

  constructor() {
    effect(() => this.redirectForRole(this.auth.userRole()));
  }

  private redirectForRole(role: string | null): void {
    if (!role) return;

    switch(role) {
      case 'Admin':            this.router.navigate(['/admin']); break;
      case 'ProductManager':   this.router.navigate(['/product-manager']); break;
      case 'ContentExecutive': this.router.navigate(['/content-executive']); break;
      case 'Customer':         this.router.navigate(['/my-account']); break;
      default:                 this.router.navigate(['/']);
    }
  }
}
