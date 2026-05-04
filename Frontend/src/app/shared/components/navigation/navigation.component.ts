import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { User } from '../../models/user.model';
import { AuthStateService } from '../../../core/state/auth-state.service';

@Component({
  selector: 'app-navigation',
  imports: [CommonModule, RouterModule],
  templateUrl: './navigation.component.html',
  styleUrls: ['./navigation.component.css']
})
export class NavigationComponent implements OnInit {
  private auth = inject(AuthStateService);

  isAuthenticated = this.auth.isAuthenticated;
  currentUser = this.auth.user;
  userRole = this.auth.userRole;
  
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
    this.auth.logout();
    this.closeMobileMenu();
  }

  getDisplayName(user: User): string {
    return user.fullName || user.email;
  }
}
