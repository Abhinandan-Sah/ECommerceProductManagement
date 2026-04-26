import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CatalogService } from '../../catalog/services/catalog.service';
import { CategoryService } from '../../catalog/services/category.service';
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
  }
}
