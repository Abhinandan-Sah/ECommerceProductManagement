import { Component, OnInit, inject } from '@angular/core';

import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { User } from '../../../shared/models/user.model';

/**
 * ViewProfileComponent displays the current user's profile information.
 *
 * Features:
 * - Displays user email, fullName, role, and account status
 * - Loading state while fetching profile data
 * - Navigation to edit profile and change password pages
 * - Error handling for profile fetch failures
 *
 * Requirements: 7.1, 7.2, 15.3
 */
@Component({
  selector: 'app-view-profile',
  imports: [RouterModule],
  templateUrl: './view-profile.component.html',
  styleUrls: ['./view-profile.component.css']
})
export class ViewProfileComponent implements OnInit {
  private userService = inject(UserService);
  private notify      = inject(NotificationService);
  private router      = inject(Router);

  user: User | null = null;
  isLoading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.loadProfile();
  }

  /**
   * Load user profile data from the API.
   */
  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.userService.getProfile().subscribe({
      next: (user) => {
        this.user = user;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.handleError(error);
      }
    });
  }

  /**
   * Handle profile loading errors.
   */
  private handleError(error: any): void {
    if (error.status === 401) {
      this.errorMessage = 'Session expired. Please log in again.';
    } else if (error.status === 0) {
      this.errorMessage = 'Network error. Please check your connection.';
    } else {
      this.errorMessage = error.error?.message || 'Failed to load profile. Please try again.';
    }

    this.notify.showError(this.errorMessage);
  }

  /**
   * Navigate to edit profile page.
   */
  editProfile(): void {
    this.router.navigate(['/profile/edit']);
  }

  /**
   * Navigate to change password page.
   */
  changePassword(): void {
    this.router.navigate(['/profile/change-password']);
  }

  /**
   * Get display text for account status.
   */
  getStatusText(): string {
    return this.user?.isActive ? 'Active' : 'Inactive';
  }

  /**
   * Get CSS class for status badge.
   */
  getStatusClass(): string {
    return this.user?.isActive ? 'status-active' : 'status-inactive';
  }
}
