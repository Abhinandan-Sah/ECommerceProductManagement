import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { take } from 'rxjs/operators';

import { UserService }        from '../../../core/services/user.service';
import { NotificationService } from '../../../core/services/notification.service';
import { User, Role, PaginatedResponse } from '../../../shared/models/user.model';
import { selectCurrentUser }  from '../../../store/auth/auth.selectors';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit, OnDestroy {
  private userService = inject(UserService);
  private store       = inject(Store);
  private notify      = inject(NotificationService);

  users: User[]    = [];
  isLoading        = true;
  errorMessage     = '';

  // Pagination
  currentPage  = 1;
  pageSize     = 10;
  totalUsers   = 0;
  totalPages   = 0;

  // Search & filter
  searchControl = new FormControl('');
  roleFilter    = new FormControl('');

  // All possible roles for the filter dropdown and the role-change select
  availableRoles = Object.values(Role);  // ['Admin','ProductManager','ContentExecutive','Customer']

  // ID of the currently logged-in user — used to lock self-modification
  currentUserId: string | null = null;

  // Delete confirmation state
  showDeleteConfirm = false;
  userToDelete: User | null = null;

  // Expose Math so the template can call Math.min()
  readonly Math = Math;

  private destroy$ = new Subject<void>();

  // ─────────────────────────────────────────────────────────────
  // Lifecycle
  // ─────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.loadCurrentUserId();
    this.setupFilters();
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ─────────────────────────────────────────────────────────────
  // Initialisation helpers
  // ─────────────────────────────────────────────────────────────

  private loadCurrentUserId(): void {
    this.store.select(selectCurrentUser).pipe(take(1)).subscribe(user => {
      this.currentUserId = user?.id ?? null;
    });
  }

  private setupFilters(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => { this.currentPage = 1; this.loadUsers(); });

    this.roleFilter.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => { this.currentPage = 1; this.loadUsers(); });
  }

  // ─────────────────────────────────────────────────────────────
  // Data loading
  // ─────────────────────────────────────────────────────────────

  loadUsers(): void {
    this.isLoading   = true;
    this.errorMessage = '';

    const search = this.searchControl.value || undefined;
    const role   = this.roleFilter.value   || undefined;

    this.userService.getUsers(this.currentPage, this.pageSize, search, role)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res: PaginatedResponse<User>) => {
          // The backend returns Role as a string ('Admin', 'ProductManager', etc.).
          // Guard against environments that may return a numeric enum value.
          const numericToRole: Record<string, Role> = {
            '1': Role.Admin,
            '2': Role.ProductManager,
            '3': Role.ContentExecutive,
            '4': Role.Customer,
          };

          this.users = res.data.map(u => {
            const raw = String(u.role);
            const normalized =
              numericToRole[raw] ??
              (Object.values(Role).includes(u.role) ? u.role : Role.Customer);
            return { ...u, role: normalized };
          });

          this.totalUsers  = res.total;
          this.totalPages  = res.totalPages;
          this.currentPage = res.page;
          this.isLoading   = false;
        },
        error: err => {
          this.isLoading = false;
          this.handleError(err, 'Failed to load users');
        }
      });
  }

  // ─────────────────────────────────────────────────────────────
  // Role management
  // ─────────────────────────────────────────────────────────────

  /**
   * Called when the admin selects a new role from the dropdown.
   * Optimistically updates the local user object so the coloured
   * badge refreshes instantly via [attr.data-role] binding;
   * rolls back on API error.
   */
  onRoleChange(user: User, newRole: string): void {
    if (this.isCurrentUser(user)) {
      this.notify.showError('You cannot change your own role');
      return;
    }

    if (user.role === newRole) return;

    const previousRole = user.role;

    // Optimistic update — refreshes the [attr.data-role] binding immediately
    user.role = newRole as Role;

    this.userService.updateUserRole(user.id, newRole)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notify.showSuccess(`Role updated to ${newRole}`);
        },
        error: err => {
          // Roll back on failure
          user.role = previousRole;
          this.handleError(err, 'Failed to update user role');
        }
      });
  }

  // ─────────────────────────────────────────────────────────────
  // Status management
  // ─────────────────────────────────────────────────────────────

  toggleUserStatus(user: User): void {
    if (this.isCurrentUser(user)) {
      this.notify.showError('You cannot change your own status');
      return;
    }

    const newStatus = !user.isActive;
    const label     = newStatus ? 'activated' : 'deactivated';

    // Optimistic update
    user.isActive = newStatus;

    this.userService.updateUserStatus(user.id, newStatus)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.notify.showSuccess(`User ${label} successfully`),
        error: err => {
          user.isActive = !newStatus;   // roll back
          this.handleError(err, `Failed to ${label.slice(0, -1)} user`);
        }
      });
  }

  // ─────────────────────────────────────────────────────────────
  // Delete
  // ─────────────────────────────────────────────────────────────

  confirmDelete(user: User): void {
    if (this.isCurrentUser(user)) {
      this.notify.showError('You cannot delete your own account');
      return;
    }
    this.userToDelete     = user;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.showDeleteConfirm = false;
    this.userToDelete      = null;
  }

  executeDelete(): void {
    if (!this.userToDelete) return;

    const { id, email } = this.userToDelete;

    this.userService.deleteUser(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notify.showSuccess(`User ${email} deleted`);
          this.cancelDelete();
          this.loadUsers();
        },
        error: err => {
          this.handleError(err, 'Failed to delete user');
          this.cancelDelete();
        }
      });
  }

  // ─────────────────────────────────────────────────────────────
  // Pagination
  // ─────────────────────────────────────────────────────────────

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    this.loadUsers();
  }

  previousPage(): void { this.goToPage(this.currentPage - 1); }
  nextPage():     void { this.goToPage(this.currentPage + 1); }

  getPageNumbers(): number[] {
    const max   = 5;
    let start   = Math.max(1, this.currentPage - Math.floor(max / 2));
    let end     = Math.min(this.totalPages, start + max - 1);
    if (end - start < max - 1) start = Math.max(1, end - max + 1);

    const pages: number[] = [];
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  // ─────────────────────────────────────────────────────────────
  // Helpers
  // ─────────────────────────────────────────────────────────────

  isCurrentUser(user: User): boolean {
    return !!this.currentUserId && user.id === this.currentUserId;
  }

  getUserDisplayName(user: User): string {
    return user.fullName?.trim() || user.email;
  }

  clearFilters(): void {
    this.searchControl.setValue('');
    this.roleFilter.setValue('');
  }

  private handleError(error: any, fallback: string): void {
    let msg = fallback;
    if      (error.status === 401)        msg = 'Session expired. Please log in again.';
    else if (error.status === 403)        msg = 'You do not have permission to do that.';
    else if (error.status === 0)          msg = 'Network error. Please check your connection.';
    else if (error.error?.message)        msg = error.error.message;

    this.errorMessage = msg;
    this.notify.showError(msg);
  }
}