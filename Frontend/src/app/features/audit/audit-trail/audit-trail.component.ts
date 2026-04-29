import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuditService } from '../services/audit.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuditLog } from '../models/audit.model';

interface GroupedAuditLogs {
  date: string;
  logs: AuditLog[];
}

@Component({
  selector: 'app-audit-trail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-trail.component.html',
  styleUrls: ['./audit-trail.component.css']
})
export class AuditTrailComponent implements OnInit {
  @Input() productId?: string;
  @Input() userId?: string;

  private auditService = inject(AuditService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);

  auditLogs: AuditLog[] = [];
  groupedAuditLogs: GroupedAuditLogs[] = [];
  isLoading = false;

  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;

  // User information for user-specific audit trail
  userName: string = '';
  userRole: string = '';

  // Filter controls
  selectedEntityType: string = '';
  selectedActionType: string = '';

  // Available filter options
  entityTypes: string[] = ['Product', 'Category', 'User', 'ProductVariant', 'MediaAsset'];
  actionTypes: string[] = ['Created', 'Updated', 'Deleted', 'StatusChanged', 'Published', 'Approved', 'Rejected'];

  // Expose Math for template usage
  Math = Math;

  ngOnInit(): void {
    // Check for query parameters (userId, userName, userRole)
    this.route.queryParams.subscribe(params => {
      if (params['userId']) {
        this.userId = params['userId'];
        this.userName = params['userName'] || '';
        this.userRole = params['userRole'] || '';
      }
    });

    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.isLoading = true;
    
    let request;
    if (this.productId) {
      request = this.auditService.getProductAuditLogs(this.productId, this.currentPage, this.pageSize);
    } else if (this.userId) {
      request = this.auditService.getUserAuditLogs(this.userId, this.currentPage, this.pageSize);
    } else {
      request = this.auditService.getAllAuditLogs(this.currentPage, this.pageSize);
    }

    request.subscribe({
      next: (data) => {
        this.auditLogs = data.items;
        this.applyFilters();
        this.totalCount = data.totalCount;
        this.totalPages = data.totalPages;
        this.isLoading = false;
      },
      error: () => {
        this.notify.showError('Failed to load audit logs');
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    let filtered = [...this.auditLogs];

    // Filter by entity type
    if (this.selectedEntityType) {
      filtered = filtered.filter(log => log.entityName === this.selectedEntityType);
    }

    // Filter by action type
    if (this.selectedActionType) {
      filtered = filtered.filter(log => log.action === this.selectedActionType);
    }

    // Group by date
    this.groupedAuditLogs = this.groupByDate(filtered);
  }

  groupByDate(logs: AuditLog[]): GroupedAuditLogs[] {
    const groups = new Map<string, AuditLog[]>();

    logs.forEach(log => {
      const date = new Date(log.createdAt);
      const dateKey = date.toLocaleDateString('en-US', { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
      });

      if (!groups.has(dateKey)) {
        groups.set(dateKey, []);
      }
      groups.get(dateKey)!.push(log);
    });

    return Array.from(groups.entries()).map(([date, logs]) => ({
      date,
      logs
    }));
  }

  onFilterChange(): void {
    this.applyFilters();
  }

  clearFilters(): void {
    this.selectedEntityType = '';
    this.selectedActionType = '';
    this.applyFilters();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadAuditLogs();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatJson(jsonString: string | null): string {
    if (!jsonString) return 'N/A';
    try {
      const obj = JSON.parse(jsonString);
      return JSON.stringify(obj, null, 2);
    } catch {
      return jsonString;
    }
  }

  formatDiff(oldValues: string | null, newValues: string | null): { field: string; oldValue: string; newValue: string }[] {
    if (!oldValues && !newValues) return [];

    try {
      const oldObj = oldValues ? JSON.parse(oldValues) : {};
      const newObj = newValues ? JSON.parse(newValues) : {};

      const allKeys = new Set([...Object.keys(oldObj), ...Object.keys(newObj)]);
      const changes: { field: string; oldValue: string; newValue: string }[] = [];

      allKeys.forEach(key => {
        const oldVal = oldObj[key] !== undefined ? String(oldObj[key]) : 'N/A';
        const newVal = newObj[key] !== undefined ? String(newObj[key]) : 'N/A';

        if (oldVal !== newVal) {
          changes.push({
            field: key,
            oldValue: oldVal,
            newValue: newVal
          });
        }
      });

      return changes;
    } catch {
      return [];
    }
  }
}
