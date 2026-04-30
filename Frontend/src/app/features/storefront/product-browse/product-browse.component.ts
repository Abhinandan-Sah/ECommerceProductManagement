import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../../catalog/services/catalog.service';
import { CategoryService } from '../../catalog/services/category.service';
import { MediaAssetService } from '../../catalog/services/media-asset.service';
import { ProductResponse, PublishStatus } from '../../catalog/models/product.model';
import { CategoryResponse } from '../../catalog/models/category.model';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

interface ProductCard extends ProductResponse {
  imageUrl?: string;
}

@Component({
  selector: 'app-product-browse',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-browse.component.html',
  styleUrls: ['./product-browse.component.css']
})
export class ProductBrowseComponent implements OnInit {
  private catalogService = inject(CatalogService);
  private categoryService = inject(CategoryService);
  private mediaService = inject(MediaAssetService);

  allProducts: ProductCard[] = [];
  filteredProducts: ProductCard[] = [];
  categories: CategoryResponse[] = [];

  isLoading = true;
  searchQuery = '';
  selectedCategoryId = '';
  selectedBrand = '';
  sortBy: 'name' | 'brand' | 'categoryName' = 'name';
  viewMode: 'grid' | 'list' = 'grid';

  get brands(): string[] {
    return [...new Set(this.allProducts.map(p => p.brand).filter(Boolean))].sort();
  }

  get totalCount(): number { return this.filteredProducts.length; }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    forkJoin({
      products: this.catalogService.getProducts().pipe(catchError(() => of([]))),
      categories: this.categoryService.getCategories().pipe(catchError(() => of([])))
    }).subscribe(({ products, categories }) => {
      this.categories = categories;
      const visibleProducts = products.filter(p =>
        p.publishStatus === PublishStatus.Approved ||
        p.publishStatus === PublishStatus.Published
      );

      // Fetch first media image for each product
      const mediaFetches = visibleProducts.map(p =>
        this.mediaService.getMediaByProduct(p.id).pipe(
          map(media => {
            const sorted = media.sort((a, b) => a.sortOrder - b.sortOrder);
            return { ...p, imageUrl: sorted[0]?.url ?? '' } as ProductCard;
          }),
          catchError(() => of({ ...p, imageUrl: '' } as ProductCard))
        )
      );

      if (mediaFetches.length === 0) {
        this.allProducts = [];
        this.applyFilters();
        this.isLoading = false;
        return;
      }

      forkJoin(mediaFetches).subscribe(cards => {
        this.allProducts = cards;
        this.applyFilters();
        this.isLoading = false;
      });
    });
  }

  applyFilters() {
    let result = [...this.allProducts];

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      result = result.filter(p =>
        p.name.toLowerCase().includes(q) ||
        p.brand?.toLowerCase().includes(q) ||
        p.categoryName?.toLowerCase().includes(q)
      );
    }

    if (this.selectedCategoryId) {
      result = result.filter(p => p.categoryId === this.selectedCategoryId);
    }

    if (this.selectedBrand) {
      result = result.filter(p => p.brand === this.selectedBrand);
    }

    result.sort((a, b) => {
      const va = (String(a[this.sortBy] ?? '')).toLowerCase();
      const vb = (String(b[this.sortBy] ?? '')).toLowerCase();
      return va.localeCompare(vb);
    });

    this.filteredProducts = result;
  }

  clearFilters() {
    this.searchQuery = '';
    this.selectedCategoryId = '';
    this.selectedBrand = '';
    this.applyFilters();
  }

  getInitials(name: string): string {
    return name.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase();
  }
}
