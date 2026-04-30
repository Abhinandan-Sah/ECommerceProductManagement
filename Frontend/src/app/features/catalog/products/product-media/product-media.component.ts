import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';

import { CatalogService } from '../../services/catalog.service';
import { MediaManagementComponent } from '../media-management/media-management.component';

@Component({
  selector: 'app-product-media',
  standalone: true,
  imports: [CommonModule, RouterModule, MediaManagementComponent],
  templateUrl: './product-media.component.html',
  styleUrls: ['./product-media.component.css']
})
export class ProductMediaComponent implements OnInit {
  private catalogService = inject(CatalogService);
  private route = inject(ActivatedRoute);

  productId!: string;
  productName = 'Loading...';

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('productId')!;

    this.catalogService.getProductById(this.productId).subscribe({
      next: (data) => {
        this.productName = data.name;
      },
      error: () => {
        this.productName = 'Product Details Unavailable';
      }
    });
  }
}
