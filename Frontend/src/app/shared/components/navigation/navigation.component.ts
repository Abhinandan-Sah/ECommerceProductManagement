import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, AsyncPipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { User } from '../../models/user.model';
import { selectCurrentUser, selectIsAuthenticated, selectUserRole } from '../../../store/auth/auth.selectors';
import { logout } from '../../../store/auth/auth.actions';

@Component({
  selector: 'app-navigation',
  imports: [CommonModule, RouterModule, AsyncPipe],
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css']
})
export class NavigationComponent implements OnInit {
  private store  = inject(Store);
  private router = inject(Router);

  isAuthenticated$: Observable<boolean>    = this.store.select(selectIsAuthenticated);
  currentUser$: Observable<User | null>    = this.store.select(selectCurrentUser);
  userRole$: Observable<string | null>     = this.store.select(selectUserRole);
  
  isMobileMenuOpen = false;
  isDarkMode = false;

  ngOnInit(): void {
    const saved = localStorage.getItem('theme');
    if (saved === 'dark') {
      this.isDarkMode = true;
      document.documentElement.setAttribute('data-theme', 'dark');
    }
  }

  toggleTheme(): void {
    this.isDarkMode = !this.isDarkMode;
    if (this.isDarkMode) {
      document.documentElement.setAttribute('data-theme', 'dark');
      localStorage.setItem('theme', 'dark');
    } else {
      document.documentElement.removeAttribute('data-theme');
      localStorage.setItem('theme', 'light');
    }
  }

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

  getDisplayName(user: User): string {
    return user.fullName || user.email;
  }
}
