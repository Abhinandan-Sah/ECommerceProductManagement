import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CatalogService } from '../../catalog/services/catalog.service';
import { CategoryService } from '../../catalog/services/category.service';
import { UserService } from '../../../core/services/user.service';
import { ProductResponse, PublishStatus } from '../../catalog/models/product.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {
  private catalogService  = inject(CatalogService);
  private categoryService = inject(CategoryService);
  private userService     = inject(UserService);

  totalUsers      = 0;
  userBreakdown: Record<string, number> = {};

  totalProducts   = 0;
  publishedCount  = 0;
  draftCount      = 0;
  recentProducts: ProductResponse[] = [];

  publishStatusEnum = PublishStatus;

  ngOnInit(): void {
    this.catalogService.getProducts().subscribe({
      next: (products) => {
        this.totalProducts  = products.length;
        this.publishedCount = products.filter(p => p.publishStatus === PublishStatus.Published).length;
        this.draftCount     = products.filter(p => p.publishStatus === PublishStatus.Draft).length;
        this.recentProducts = [...products].reverse().slice(0, 5);
      },
      error: () => {}
    });

    this.userService.getUserStats().subscribe({
      next: (stats) => {
        this.userBreakdown = stats;
        this.totalUsers = Object.values(stats).reduce((a, b) => a + b, 0);
      },
      error: () => {}
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
