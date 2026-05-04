import { Component, OnInit, inject } from '@angular/core';

import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { User } from '../../../shared/models/user.model';

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

  editProfile(): void {
    this.router.navigate(['/profile/edit']);
  }

  changePassword(): void {
    this.router.navigate(['/profile/change-password']);
  }

  getStatusText(): string {
    return this.user?.isActive ? 'Active' : 'Inactive';
  }

  getStatusClass(): string {
    return this.user?.isActive ? 'status-active' : 'status-inactive';
  }
}
