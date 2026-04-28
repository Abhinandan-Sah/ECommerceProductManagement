import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ReportingService } from '../services/reporting.service';
import { NotificationService } from '../../../core/services/notification.service';
import { DashboardKpi, ProductReport, PagedResult } from '../models/reporting.model';

@Component({
  selector: 'app-reporting-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reporting-dashboard.component.html',
  styleUrls: ['./reporting-dashboard.component.css']
})
export class ReportingDashboardComponent implements OnInit {
  private reportingService = inject(ReportingService);
  private notify = inject(NotificationService);

  kpi: DashboardKpi | null = null;
  productReports: ProductReport[] = [];
  
  isLoadingKpi = false;
  isLoadingReports = false;
  isExporting = false;

  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;

  // Filters
  categoryFilter = new FormControl('');
  statusFilter = new FormControl('');

  // Expose Math for template usage
  Math = Math;

  ngOnInit(): void {
    this.loadDashboardKpi();
    this.loadProductReports();
  }

  loadDashboardKpi(): void {
    this.isLoadingKpi = true;
    this.reportingService.getDashboardKpi().subscribe({
      next: (data) => {
        this.kpi = data;
        this.isLoadingKpi = false;
      },
      error: () => {
        this.notify.showError('Failed to load dashboard KPIs');
        this.isLoadingKpi = false;
      }
    });
  }

  loadProductReports(): void {
    this.isLoadingReports = true;
    const filter = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      category: this.categoryFilter.value || undefined,
      status: this.statusFilter.value || undefined
    };

    this.reportingService.getProductReports(filter).subscribe({
      next: (data: PagedResult<ProductReport>) => {
        this.productReports = data.items;
        this.totalCount = data.totalCount;
        this.totalPages = data.totalPages;
        this.isLoadingReports = false;
      },
      error: () => {
        this.notify.showError('Failed to load product reports');
        this.isLoadingReports = false;
      }
    });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadProductReports();
  }

  clearFilters(): void {
    this.categoryFilter.setValue('');
    this.statusFilter.setValue('');
    this.applyFilters();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadProductReports();
  }

  exportToCsv(): void {
    this.isExporting = true;
    this.reportingService.exportProductsCsv().subscribe({
      next: (blob) => {
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'products_report.csv';
        link.click();
        
        // Cleanup
        window.URL.revokeObjectURL(url);
        
        this.notify.showSuccess('Report exported successfully');
        this.isExporting = false;
      },
      error: () => {
        this.notify.showError('Failed to export report');
        this.isExporting = false;
      }
    });
  }

  formatDate(dateString: string | null): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }
}
