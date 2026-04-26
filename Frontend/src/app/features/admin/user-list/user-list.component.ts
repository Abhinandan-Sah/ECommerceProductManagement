import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { take } from 'rxjs/operators';

import { UserService } from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { User, Role, PaginatedResponse } from '../../../shared/models/user.model';
import { selectCurrentUser } from '../../../store/auth/auth.selectors';

/**
 * UserListComponent provides admin interface for managing all users.
 *
 * Features:
 * - Paginated user list with search and role filtering
 * - Role management (change user roles)
 * - Status management (activate/deactivate users)
 * - User deletion with confirmation
 * - Prevents admins from modifying their own account
 *
 * Requirements: 9.1-9.6, 10.1-10.6, 11.1-11.6, 12.1-12.6, 15.3
 */
@Component({
  selector: 'app-user-list',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit, OnDestroy {
  private userService  = inject(UserService);
  private store        = inject(Store);
  private notify       = inject(NotificationService);
  private router       = inject(Router);

  users: User[] = [];
  isLoading = true;
  errorMessage = '';

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalUsers = 0;
  totalPages = 0;

  // Search and filter
  searchControl = new FormControl('');
  roleFilter = new FormControl('');

  // Available roles for filtering and role management
  availableRoles = Object.values(Role);

  // Current user ID to prevent self-modification
  currentUserId: string | null = null;

  // Confirmation dialog state
  showDeleteConfirm = false;
  userToDelete: User | null = null;

  // Unsubscribe subject
  private destroy$ = new Subject<void>();

  // Expose Math for template
  Math = Math;

  ngOnInit(): void {
    this.initializeCurrentUser();
    this.setupSearchAndFilter();
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Get current user ID to prevent self-modification.
   */
  private initializeCurrentUser(): void {
    this.store.select(selectCurrentUser).pipe(
      take(1)
    ).subscribe(user => {
      this.currentUserId = user?.id ?? null;
    });
  }

  /**
   * Set up search and filter with debouncing.
   */
  private setupSearchAndFilter(): void {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.loadUsers();
      });

    this.roleFilter.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 1;
        this.loadUsers();
      });
  }

  /**
   * Load users from API with current pagination, search, and filter settings.
   */
  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const search = this.searchControl.value || undefined;
    const role = this.roleFilter.value || undefined;

    this.userService.getUsers(this.currentPage, this.pageSize, search, role)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: PaginatedResponse<User>) => {
          this.users = response.data;
          this.totalUsers = response.total;
          this.totalPages = response.totalPages;
          this.currentPage = response.page;
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.handleError(error, 'Failed to load users');
        }
      });
  }

  /**
   * Navigate to specific page.
   */
  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) {
      return;
    }
    this.currentPage = page;
    this.loadUsers();
  }

  /**
   * Navigate to previous page.
   */
  previousPage(): void {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  /**
   * Navigate to next page.
   */
  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  /**
   * Get array of page numbers for pagination controls.
   */
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPagesToShow = 5;

    let startPage = Math.max(1, this.currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  /**
   * Check if user is the current logged-in user.
   */
  isCurrentUser(user: User): boolean {
    return user.id === this.currentUserId;
  }

  /**
   * Handle role change for a user.
   */
  onRoleChange(user: User, newRole: string): void {
    if (this.isCurrentUser(user)) {
      this.notify.showError('You cannot change your own role');
      return;
    }

    if (user.role === newRole) {
      return;
    }

    this.userService.updateUserRole(user.id, newRole)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notify.showSuccess(`User role updated to ${newRole}`);
          this.loadUsers();
        },
        error: (error) => {
          this.handleError(error, 'Failed to update user role');
          this.loadUsers();
        }
      });
  }

  /**
   * Toggle user active status.
   */
  toggleUserStatus(user: User): void {
    if (this.isCurrentUser(user)) {
      this.notify.showError('You cannot change your own status');
      return;
    }

    const newStatus = !user.isActive;
    const action = newStatus ? 'activated' : 'deactivated';

    this.userService.updateUserStatus(user.id, newStatus)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notify.showSuccess(`User ${action} successfully`);
          this.loadUsers();
        },
        error: (error) => {
          this.handleError(error, `Failed to ${action.slice(0, -1)} user`);
          this.loadUsers();
        }
      });
  }

  /**
   * Show delete confirmation dialog.
   */
  confirmDelete(user: User): void {
    if (this.isCurrentUser(user)) {
      this.notify.showError('You cannot delete your own account');
      return;
    }

    this.userToDelete = user;
    this.showDeleteConfirm = true;
  }

  /**
   * Cancel delete operation.
   */
  cancelDelete(): void {
    this.showDeleteConfirm = false;
    this.userToDelete = null;
  }

  /**
   * Execute user deletion.
   */
  executeDelete(): void {
    if (!this.userToDelete) {
      return;
    }

    const userId = this.userToDelete.id;
    const userEmail = this.userToDelete.email;

    this.userService.deleteUser(userId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notify.showSuccess(`User ${userEmail} deleted successfully`);
          this.showDeleteConfirm = false;
          this.userToDelete = null;
          this.loadUsers();
        },
        error: (error) => {
          this.handleError(error, 'Failed to delete user');
          this.showDeleteConfirm = false;
          this.userToDelete = null;
        }
      });
  }

  /**
   * Get display name for user (uses fullName field directly).
   */
  getUserDisplayName(user: User): string {
    return user.fullName || user.email;
  }

  /**
   * Get CSS class for status badge.
   */
  getStatusClass(user: User): string {
    return user.isActive ? 'status-active' : 'status-inactive';
  }

  /**
   * Get status text.
   */
  getStatusText(user: User): string {
    return user.isActive ? 'Active' : 'Inactive';
  }

  /**
   * Handle errors with user-friendly messages.
   */
  private handleError(error: any, defaultMessage: string): void {
    let errorMessage = defaultMessage;

    if (error.status === 401) {
      errorMessage = 'Session expired. Please log in again.';
    } else if (error.status === 403) {
      errorMessage = 'You do not have permission to perform this action.';
    } else if (error.status === 0) {
      errorMessage = 'Network error. Please check your connection.';
    } else if (error.error?.message) {
      errorMessage = error.error.message;
    }

    this.errorMessage = errorMessage;
    this.notify.showError(errorMessage);
  }

  /**
   * Clear search and filters.
   */
  clearFilters(): void {
    this.searchControl.setValue('');
    this.roleFilter.setValue('');
  }
}
