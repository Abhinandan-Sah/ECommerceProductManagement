import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, AsyncPipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { User } from '../../models/user.model';
import { selectCurrentUser, selectIsAuthenticated } from '../../../store/auth/auth.selectors';
import { logout } from '../../../store/auth/auth.actions';

@Component({
  selector: 'app-navigation',
  imports: [CommonModule, RouterModule, AsyncPipe],
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css']
})
export class NavigationComponent {
  private store  = inject(Store);
  private router = inject(Router);

  isAuthenticated$: Observable<boolean>    = this.store.select(selectIsAuthenticated);
  currentUser$: Observable<User | null>    = this.store.select(selectCurrentUser);
  isMobileMenuOpen = false;

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  logout(): void {
    this.store.dispatch(logout());
    this.closeMobileMenu();
  }

  isAdmin(user: User | null): boolean {
    return user?.role === 'Admin';
  }

  getDisplayName(user: User): string {
    return user.fullName || user.email;
  }
}
