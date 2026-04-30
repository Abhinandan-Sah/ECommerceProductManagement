import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';

import { CatalogService } from '../../catalog/services/catalog.service';
import { MediaAssetService } from '../../catalog/services/media-asset.service';
import { WorkflowService } from '../../workflow/services/workflow.service';
import { ProductResponse, PublishStatus } from '../../catalog/models/product.model';
import { MediaAssetResponse } from '../../catalog/models/media-asset.model';
import { ProductVariantResponse } from '../../catalog/models/product-variant.model';
import { ApprovalStatus } from '../../workflow/models/workflow.model';
import { selectUserRole } from '../../../store/auth/auth.selectors';

type InventoryInfo = {
  warehouseLocation: string;
  availableQty: number;
  reorderLevel: number;
};

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
  private router = inject(Router);
  private store = inject(Store);

  productId!: string;
  product: ProductResponse | null = null;
  mediaAssets: MediaAssetResponse[] = [];
  variants: ProductVariantResponse[] = [];
  pricing: PricingInfo | null = null;
  inventory: InventoryInfo | null = null;
  approvalStatus: ApprovalStatus | null = null;
  isBackOfficeView = false;
  isLoading = true;
  selectedImageUrl = '';
  userRole: string | null = null;
  canEditProduct = false;
  canViewInventory = false;
  canManageVariants = false;

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id')!;
    this.isBackOfficeView = this.router.url.startsWith('/catalog/');
    this.store.select(selectUserRole).subscribe(role => {
      this.userRole = role ?? null;
      this.canEditProduct = role === 'Admin' || role === 'ProductManager';
      this.canViewInventory = role === 'Admin' || role === 'ProductManager';
      this.canManageVariants = role === 'Admin' || role === 'ProductManager' || role === 'ContentExecutive';
    });
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
        if (this.isBackOfficeView) {
          if (this.canViewInventory) {
            this.loadInventory();
          }
          this.loadApprovalStatus();
        }
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

  loadInventory(): void {
    this.workflowService.getInventory(this.productId).subscribe({
      next: (inventory) => {
        this.inventory = inventory
          ? {
              warehouseLocation: inventory.warehouseLocation ?? inventory.WarehouseLocation ?? '',
              availableQty: Number(inventory.availableQty ?? inventory.AvailableQty) || 0,
              reorderLevel: Number(inventory.reorderLevel ?? inventory.ReorderLevel) || 0
            }
          : null;
      },
      error: () => {
        this.inventory = null;
      }
    });
  }

  loadApprovalStatus(): void {
    this.workflowService.getApprovalStatus(this.productId).subscribe({
      next: (status) => {
        this.approvalStatus = status?.status ?? null;
      },
      error: () => {
        this.approvalStatus = null;
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

  hasInventory(): boolean {
    if (!this.inventory) return false;
    return Boolean(this.inventory.warehouseLocation)
      || Number(this.inventory.availableQty) > 0
      || Number(this.inventory.reorderLevel) > 0;
  }

  getStockStatus(): 'Low Stock' | 'Healthy' | 'Not Set' {
    if (!this.hasInventory() || !this.inventory) return 'Not Set';
    return this.inventory.reorderLevel > 0 && this.inventory.availableQty <= this.inventory.reorderLevel
      ? 'Low Stock'
      : 'Healthy';
  }

  getPublishStatusText(status: PublishStatus): string {
    switch (status) {
      case PublishStatus.Draft: return 'Draft';
      case PublishStatus.InEnrichment: return 'In Enrichment';
      case PublishStatus.ReadyForReview: return 'Ready for Review';
      case PublishStatus.Approved: return 'Approved';
      case PublishStatus.Published: return 'Published';
      case PublishStatus.Rejected: return 'Rejected';
      case PublishStatus.Archived: return 'Archived';
      default: return 'Unknown';
    }
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
