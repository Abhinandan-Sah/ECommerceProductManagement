import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Observable } from 'rxjs';
import { User } from '../../models/user.model';

@Component({
    selector: 'app-navigation',
    imports: [CommonModule, RouterModule],
    templateUrl: './navigation.component.html',
    styleUrls: ['./navigation.component.css']
})
export class NavigationComponent implements OnInit {
  isAuthenticated$: Observable<boolean>;
  currentUser$: Observable<User | null>;
  isMobileMenuOpen = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.isAuthenticated$ = this.authService.isAuthenticated$;
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {}

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  logout(): void {
    const refreshToken = this.authService.getRefreshToken();
    if (refreshToken) {
      this.authService.logout().subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: () => {
          // Even if logout fails, clear local state and redirect
          this.authService.clearAuthState();
          this.router.navigate(['/login']);
        }
      });
    } else {
      this.authService.clearAuthState();
      this.router.navigate(['/login']);
    }
    this.closeMobileMenu();
  }

  isAdmin(user: User | null): boolean {
    return user?.role === 'Admin';
  }

  getDisplayName(user: User): string {
    const fullName = `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim();
    return fullName || user.email;
  }
}
