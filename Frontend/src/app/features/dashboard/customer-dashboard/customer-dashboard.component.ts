import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { User } from '../../../shared/models/user.model';
import { selectCurrentUser } from '../../../store/auth/auth.selectors';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.css']
})
export class CustomerDashboardComponent implements OnInit {
  private store = inject(Store);
  
  user$: Observable<User | null> = this.store.select(selectCurrentUser);
  recentOrders = []; // Mock empty for now

  ngOnInit(): void {
  }
}
