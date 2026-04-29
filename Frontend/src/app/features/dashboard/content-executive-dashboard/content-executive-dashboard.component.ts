import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CatalogService } from '../../catalog/services/catalog.service';
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
  private notification   = inject(NotificationService);

  products: ProductResponse[] = [];
  draftProducts: ProductResponse[] = [];
  
  totalCount     = 0;
  publishedToday = 0; 
  needsReview    = 0;

  publishStatusEnum = PublishStatus;
  isPublishing = false;

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.catalogService.getProducts().subscribe({
      next: (data) => {
        this.products = data;
        this.draftProducts = data.filter(p => p.publishStatus === PublishStatus.Draft);
        
        this.totalCount = data.length;
        this.needsReview = this.draftProducts.length;
        this.publishedToday = Math.floor(Math.random() * 5) + 1; // Simulated
      },
      error: () => this.notification.showError('Failed to load products')
    });
  }

  publishProduct(id: string): void {
    const product = this.products.find(p => p.id === id);
    if (!product) return;

    this.isPublishing = true;
    const dto = { 
      name: product.name, 
      brand: product.brand, 
      categoryId: product.categoryId || '',
      description: product.description,
      publishStatus: PublishStatus.Published 
    };

    this.catalogService.updateProduct(id, dto).subscribe({
      next: () => {
        this.notification.showSuccess('Product published successfully');
        this.isPublishing = false;
        this.loadProducts();
      },
      error: () => {
        this.notification.showError('Failed to publish product');
        this.isPublishing = false;
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
