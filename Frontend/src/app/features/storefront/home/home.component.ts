import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CatalogService } from '../../catalog/services/catalog.service';
import { CategoryService } from '../../catalog/services/category.service';
import { MediaAssetService } from '../../catalog/services/media-asset.service';
import { ProductResponse, PublishStatus } from '../../catalog/models/product.model';
import { CategoryResponse } from '../../catalog/models/category.model';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

interface ProductCard extends ProductResponse {
  imageUrl: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  private catalogService = inject(CatalogService);
  private categoryService = inject(CategoryService);
  private mediaService   = inject(MediaAssetService);

  products: ProductCard[] = [];
  categories: CategoryResponse[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadCategories();
    this.loadFeaturedProducts();
  }

  loadCategories(): void {
    this.categoryService.getCategories().pipe(
      catchError(() => of([]))
    ).subscribe(categories => {
      this.categories = categories;
    });
  }

  loadFeaturedProducts(): void {
    this.catalogService.getProducts().subscribe({
      next: (data) => {
        const visibleProducts = data
          .filter(p =>
            p.publishStatus === PublishStatus.Approved ||
            p.publishStatus === PublishStatus.Published
          )
          .slice(0, 8);

        if (visibleProducts.length === 0) {
          this.products = [];
          this.isLoading = false;
          return;
        }

        // Fetch primary image for each product (sorted by sortOrder)
        const fetches = visibleProducts.map(p =>
          this.mediaService.getMediaByProduct(p.id).pipe(
            map(media => {
              const sorted = media.sort((a, b) => a.sortOrder - b.sortOrder);
              return { ...p, imageUrl: sorted[0]?.url ?? '' } as ProductCard;
            }),
            catchError(() => of({ ...p, imageUrl: '' } as ProductCard))
          )
        );

        forkJoin(fetches).subscribe({
          next: cards => {
            this.products = cards;
            this.isLoading = false;
          },
          error: () => {
            this.products = visibleProducts.map(p => ({ ...p, imageUrl: '' }));
            this.isLoading = false;
          }
        });
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  getInitials(name: string): string {
    return name.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase();
  }
}
