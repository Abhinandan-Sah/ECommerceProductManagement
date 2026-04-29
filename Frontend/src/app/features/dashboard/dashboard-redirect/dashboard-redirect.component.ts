import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { selectUserRole } from '../../../store/auth/auth.selectors';

@Component({
  selector: 'app-dashboard-redirect',
  standalone: true,
  template: '<div class="page-container"><p>Loading dashboard...</p></div>'
})
export class DashboardRedirectComponent implements OnInit {
  private router = inject(Router);
  private store = inject(Store);

  ngOnInit(): void {
    this.store.select(selectUserRole).subscribe(role => {
      if (!role) return;
      
      switch(role) {
        case 'Admin':            this.router.navigate(['/admin']); break;
        case 'ProductManager':   this.router.navigate(['/product-manager']); break;
        case 'ContentExecutive': this.router.navigate(['/content-executive']); break;
        case 'Customer':         this.router.navigate(['/my-account']); break;
        default:                 this.router.navigate(['/']);
      }
    });
  }
}
