import { Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditService } from '../services/audit.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuditLog } from '../models/audit.model';

@Component({
  selector: 'app-audit-trail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './audit-trail.component.html',
  styleUrls: ['./audit-trail.component.css']
})
export class AuditTrailComponent implements OnInit {
  @Input() productId?: string;
  @Input() userId?: string;

  private auditService = inject(AuditService);
  private notify = inject(NotificationService);

  auditLogs: AuditLog[] = [];
  isLoading = false;

  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;

  // Expose Math for template usage
  Math = Math;

  ngOnInit(): void {
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

  formatJson(jsonString: string | null): string {
    if (!jsonString) return 'N/A';
    try {
      const obj = JSON.parse(jsonString);
      return JSON.stringify(obj, null, 2);
    } catch {
      return jsonString;
    }
  }
}
