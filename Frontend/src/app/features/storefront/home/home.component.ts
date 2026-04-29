import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CatalogService } from '../../catalog/services/catalog.service';
import { MediaAssetService } from '../../catalog/services/media-asset.service';
import { ProductResponse, PublishStatus } from '../../catalog/models/product.model';
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
  private mediaService   = inject(MediaAssetService);

  products: ProductCard[] = [];
  isLoading = true;

  ngOnInit(): void { this.loadFeaturedProducts(); }

  loadFeaturedProducts(): void {
    this.catalogService.getProducts(undefined, PublishStatus.Published).subscribe({
      next: (data) => {
        const published = data
          .filter(p => p.publishStatus === PublishStatus.Published)
          .slice(0, 8);

        if (published.length === 0) {
          this.products = [];
          this.isLoading = false;
          return;
        }

        // Fetch primary image for each product (sorted by sortOrder)
        const fetches = published.map(p =>
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
            this.products = published.map(p => ({ ...p, imageUrl: '' }));
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
