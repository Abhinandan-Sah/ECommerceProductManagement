import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CatalogService } from '../../catalog/services/catalog.service';
import { WorkflowService } from '../../workflow/services/workflow.service';
import { ProductResponse, PublishStatus } from '../../catalog/models/product.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-content-executive-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './content-executive-dashboard.component.html',
  styleUrls: ['./content-executive-dashboard.component.css']
})
export class ContentExecutiveDashboardComponent implements OnInit {
  private catalogService = inject(CatalogService);
  private workflowService = inject(WorkflowService);
  private notification = inject(NotificationService);

  products: ProductResponse[] = []
  /** Products that can be submitted for review (Draft or InEnrichment) */
  submittableProducts: ProductResponse[] = [];

  totalCount = 0;
  submittedCount = 0;
  needsEnrichment = 0;

  publishStatusEnum = PublishStatus;
  isSubmitting = false;

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.catalogService.getProducts().subscribe({
      next: (data) => {
        this.products = data;
        // Submittable: Draft or InEnrichment — ReadyForReview already submitted
        this.submittableProducts = data.filter(
          p => p.publishStatus === PublishStatus.Draft ||
            p.publishStatus === PublishStatus.InEnrichment
        );

        this.totalCount = data.length;
        this.submittedCount = data.filter(p => p.publishStatus === PublishStatus.ReadyForReview).length;
        this.needsEnrichment = this.submittableProducts.length;
      },
      error: () => this.notification.showError('Failed to load products')
    });
  }

  /**
   * Submit a product for review via the Workflow API
   * (POST /workflow/products/{id}/submit).
   * ContentExecutive does NOT call PUT /products/{id}.
   */
  submitForReview(id: string): void {
    this.isSubmitting = true;
    this.workflowService.submitForReview(id).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.loadProducts();
      },
      error: () => {
        // WorkflowService already shows a toast on error
        this.isSubmitting = false;
      }
    });
  }

  getStatusBadgeClass(status: PublishStatus): string {
    switch (status) {
      case PublishStatus.Published: return 'badge-published';
      case PublishStatus.Draft: return 'badge-draft';
      case PublishStatus.Archived: return 'badge-archived';
      case PublishStatus.InEnrichment: return 'badge-draft';
      case PublishStatus.ReadyForReview: return 'badge-warning';
      case PublishStatus.Approved: return 'badge-success';
      case PublishStatus.Rejected: return 'badge-danger';
      default: return 'badge-draft';
    }
  }

  getStatusText(status: PublishStatus): string {
    switch (status) {
      case PublishStatus.Published: return 'Published';
      case PublishStatus.Draft: return 'Draft';
      case PublishStatus.Archived: return 'Archived';
      case PublishStatus.InEnrichment: return 'In Enrichment';
      case PublishStatus.ReadyForReview: return 'Ready for Review';
      case PublishStatus.Approved: return 'Approved';
      case PublishStatus.Rejected: return 'Rejected';
      default: return 'Unknown';
    }
  }
}
