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
}
