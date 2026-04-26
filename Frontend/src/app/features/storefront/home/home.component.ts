import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CatalogService } from '../../catalog/services/catalog.service';
import { ProductResponse, PublishStatus } from '../../catalog/models/product.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  private catalogService = inject(CatalogService);
  products: ProductResponse[] = [];
  selectedCategory = '';

  publishStatusEnum = PublishStatus;

  ngOnInit(): void {
    this.loadFeaturedProducts();
  }

  loadFeaturedProducts(): void {
    // Load only Published products for the public storefront
    this.catalogService.getProducts(undefined, PublishStatus.Published).subscribe({
      next: (data) => {
        // filter by published status specifically in case the backend didn't
        this.products = data.filter(p => p.publishStatus === PublishStatus.Published).slice(0, 8);
      },
      error: () => {}  // Fail silently on public page
    });
  }
}
