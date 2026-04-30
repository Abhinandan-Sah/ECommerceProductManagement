import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';

import { CatalogService } from '../../catalog/services/catalog.service';
import { MediaAssetService } from '../../catalog/services/media-asset.service';
import { WorkflowService } from '../../workflow/services/workflow.service';
import { ProductResponse } from '../../catalog/models/product.model';
import { MediaAssetResponse } from '../../catalog/models/media-asset.model';
import { ProductVariantResponse } from '../../catalog/models/product-variant.model';

type PricingInfo = {
  productId: string;
  mrp: number;
  salePrice: number;
  gstPercent: number;
};

type PricingApiResponse = Partial<PricingInfo> & {
  ProductId?: string;
  MRP?: number;
  SalePrice?: number;
  GSTPercent?: number;
};

type VariantApiResponse = Partial<ProductVariantResponse> & {
  Id?: string;
  ProductId?: string;
  Color?: string;
  Size?: string;
  Barcode?: string;
};

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css']
})
export class ProductDetailComponent implements OnInit {
  private catalogService = inject(CatalogService);
  private mediaService = inject(MediaAssetService);
  private workflowService = inject(WorkflowService);
  private route = inject(ActivatedRoute);

  productId!: string;
  product: ProductResponse | null = null;
  mediaAssets: MediaAssetResponse[] = [];
  variants: ProductVariantResponse[] = [];
  pricing: PricingInfo | null = null;
  isLoading = true;
  selectedImageUrl = '';

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id')!;
    this.loadProduct();
  }

  loadProduct(): void {
    this.isLoading = true;
    this.catalogService.getProductById(this.productId).subscribe({
      next: (product) => {
        this.product = product;
        this.loadMedia();
        this.loadVariants();
        this.loadPricing();
      },
      error: () => {
        this.product = null;
        this.isLoading = false;
      }
    });
  }

  loadMedia(): void {
    this.mediaService.getMediaByProduct(this.productId).subscribe({
      next: (media) => {
        this.mediaAssets = [...media].sort((a, b) => a.sortOrder - b.sortOrder);
        this.selectedImageUrl = this.mediaAssets[0]?.url ?? '';
        this.isLoading = false;
      },
      error: () => {
        this.mediaAssets = [];
        this.selectedImageUrl = '';
        this.isLoading = false;
      }
    });
  }

  loadVariants(): void {
    this.catalogService.getVariants(this.productId).subscribe({
      next: (variants) => {
        this.variants = this.normalizeVariants(variants);
      },
      error: () => {
        this.variants = [];
      }
    });
  }

  loadPricing(): void {
    this.workflowService.getPricing(this.productId).subscribe({
      next: (pricing) => {
        this.pricing = this.normalizePricing(pricing);
      },
      error: () => {
        this.pricing = null;
      }
    });
  }

  selectImage(url: string): void {
    this.selectedImageUrl = url;
  }

  getDiscountAmount(): number {
    if (!this.pricing) return 0;
    const mrp = Number(this.pricing.mrp) || 0;
    const sale = Number(this.pricing.salePrice) || 0;
    return mrp > sale ? mrp - sale : 0;
  }

  getDiscountPercent(): number {
    if (!this.pricing) return 0;
    const mrp = Number(this.pricing.mrp) || 0;
    const discount = this.getDiscountAmount();
    return mrp > 0 ? Math.round((discount / mrp) * 100) : 0;
  }

  formatPrice(value: number): string {
    const numeric = Number(value) || 0;
    return numeric.toFixed(2);
  }

  hasPricing(): boolean {
    if (!this.pricing) return false;
    return Number(this.pricing.mrp) > 0 || Number(this.pricing.salePrice) > 0;
  }

  private normalizePricing(pricing: PricingApiResponse | null | undefined): PricingInfo | null {
    if (!pricing) return null;

    return {
      productId: pricing.productId ?? pricing.ProductId ?? this.productId,
      mrp: Number(pricing.mrp ?? pricing.MRP) || 0,
      salePrice: Number(pricing.salePrice ?? pricing.SalePrice) || 0,
      gstPercent: Number(pricing.gstPercent ?? pricing.GSTPercent) || 0
    };
  }

  private normalizeVariants(response: ProductVariantResponse[] | { items?: VariantApiResponse[] } | null | undefined): ProductVariantResponse[] {
    const variants: VariantApiResponse[] = Array.isArray(response) ? response : response?.items ?? [];

    return variants
      .map((variant) => ({
        id: variant.id ?? variant.Id ?? '',
        productId: variant.productId ?? variant.ProductId ?? this.productId,
        color: variant.color ?? variant.Color ?? '',
        size: variant.size ?? variant.Size ?? '',
        barcode: variant.barcode ?? variant.Barcode ?? ''
      }))
      .filter((variant) => variant.id || variant.color || variant.size || variant.barcode);
  }
}
