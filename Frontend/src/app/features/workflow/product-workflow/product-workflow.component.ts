import { Component, OnInit, OnDestroy, inject, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, FormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { selectUserRole } from '../../../store/auth/auth.selectors';
import { WorkflowService } from '../services/workflow.service';
import { NotificationService } from '../../../core/services/notification.service';
import { CatalogService } from '../../catalog/services/catalog.service';
import { MediaAssetService } from '../../catalog/services/media-asset.service';
import { ApprovalStatus } from '../models/workflow.model';
import { ProductResponse } from '../../catalog/models/product.model';
import { FormControl } from '@angular/forms';

export interface ProductWithImage extends ProductResponse {
  imageUrl?: string;
}

@Component({
  selector: 'app-product-workflow',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './product-workflow.component.html',
  styleUrls: ['./product-workflow.component.css']
})
export class ProductWorkflowComponent implements OnInit, OnDestroy {
  private store = inject(Store);
  private workflowService = inject(WorkflowService);
  private notificationService = inject(NotificationService);
  private catalogService = inject(CatalogService);
  private mediaService = inject(MediaAssetService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private el = inject(ElementRef);

  userRole: string | null = null;
  private roleSub?: Subscription;

  canManagePricing = false;
  canManageInventory = false;
  canSubmit = false;
  canUpdateStatus = false;

  // Product picker state
  productSearch = '';
  allProducts: ProductWithImage[] = [];
  filteredProducts: ProductWithImage[] = [];
  selectedProduct: ProductWithImage | null = null;
  showDropdown = false;
  isLoadingProducts = false;

  productIdControl = this.fb.control('', Validators.required);

  pricingForm: FormGroup;
  inventoryForm: FormGroup;
  statusForm: FormGroup;

  activeTab: 'Pricing' | 'Inventory' | 'Approval' = 'Approval';
  currentApprovalStatus: ApprovalStatus = 'Pending';
  
  // Summary data for review
  currentPricing: any = null;
  currentInventory: any = null;

  isSavingPricing = false;
  isSavingInventory = false;
  isSubmitting = false;
  isUpdatingStatus = false;

  readonly statusFlow: { key: ApprovalStatus; label: string }[] = [
    { key: 'Pending',  label: 'Pending' },
    { key: 'Approved', label: 'Approved' },
    { key: 'Rejected', label: 'Rejected' },
  ];

  private readonly statusOrder: ApprovalStatus[] = [
    'Pending', 'Approved', 'Rejected'
  ];

  constructor() {
    this.pricingForm = this.fb.group({
      mrp:        ['', [Validators.required, Validators.min(0.01)]],
      salePrice:  ['', [Validators.required, Validators.min(0.01)]],
      gstPercent: ['', [Validators.required, Validators.min(0), Validators.max(100)]]
    }, { validators: this.priceValidator });

    this.inventoryForm = this.fb.group({
      warehouseLocation: ['', [Validators.required, Validators.maxLength(200)]],
      availableQty:      ['', [Validators.required, Validators.min(0)]],
      reorderLevel:      ['', [Validators.required, Validators.min(0)]]
    });

    this.statusForm = this.fb.group({
      newStatus: ['Pending', Validators.required],
      remarks:   ['', [Validators.maxLength(500)]]
    }, { validators: this.remarksValidator });
  }

  ngOnInit() {
    this.roleSub = this.store.select(selectUserRole).subscribe(role => {
      this.userRole = role;
      this.canManagePricing   = role === 'Admin' || role === 'ProductManager';
      this.canManageInventory = role === 'Admin' || role === 'ProductManager';
      this.canSubmit          = role === 'Admin' || role === 'ProductManager' || role === 'ContentExecutive';
      this.canUpdateStatus    = role === 'Admin';

      if (this.canManagePricing)        this.activeTab = 'Pricing';
      else if (this.canManageInventory) this.activeTab = 'Inventory';
      else                              this.activeTab = 'Approval';
    });

    this.route.queryParams.subscribe(params => {
      if (params['productId']) {
        this.productIdControl.setValue(params['productId']);
        if (params['tab'] === 'inventory') this.activeTab = 'Inventory';
        if (params['tab'] === 'Approval') this.activeTab = 'Approval';
      }
    });

    // Load all products for the picker
    this.loadProducts();
  }

  ngOnDestroy() { this.roleSub?.unsubscribe(); }

  // ---- Product Picker ----
  loadProducts() {
    this.isLoadingProducts = true;
    this.catalogService.getProducts().subscribe({
      next: products => {
        this.allProducts = products;
        this.filteredProducts = products;
        this.isLoadingProducts = false;

        // If a productId was pre-loaded (via query param), resolve the product name
        const preId = this.productIdControl.value;
        if (preId) {
          const found = this.allProducts.find(p => p.id === preId);
          if (found) {
            this.selectedProduct = found;
            this.fetchReviewData(preId);
          }
        }
      },
      error: () => { this.isLoadingProducts = false; }
    });
  }

  onSearchInput() {
    const q = this.productSearch.toLowerCase();
    this.filteredProducts = this.allProducts.filter(p =>
      p.name.toLowerCase().includes(q) ||
      p.brand?.toLowerCase().includes(q) ||
      p.categoryName?.toLowerCase().includes(q) ||
      p.sku?.toLowerCase().includes(q)
    );
    this.showDropdown = true;
  }

  selectProduct(product: ProductWithImage) {
    this.selectedProduct = product;
    this.productIdControl.setValue(product.id);
    this.productSearch = product.name;
    this.showDropdown = false;
    // Reset forms when a new product is picked
    this.pricingForm.reset();
    this.inventoryForm.reset();
    this.statusForm.reset({ newStatus: 'Pending' });
    this.fetchReviewData(product.id);
  }

  fetchReviewData(productId: string) {
    this.workflowService.getApprovalStatus(productId).subscribe(status => {
      this.currentApprovalStatus = status?.status ?? 'Pending';
    });

    // We don't have a direct "getPricing" but we can check if it exists or use the summary logic
    // For now, let's assume we fetch them to show in the Approval tab
    this.workflowService.getInventory(productId).subscribe(inv => {
      this.currentInventory = inv;
      if (inv) {
        this.inventoryForm.patchValue({
          warehouseLocation: inv.warehouseLocation,
          availableQty: inv.availableQty,
          reorderLevel: inv.reorderLevel
        });
      }
    });
    
    this.workflowService.getPricing(productId).subscribe(prc => {
      this.currentPricing = prc;
      if (prc) {
        this.pricingForm.patchValue({
          mrp: prc.mrp,
          salePrice: prc.salePrice,
          gstPercent: prc.gstPercent
        });
      }
    });
  }

  clearSelection() {
    this.selectedProduct = null;
    this.productIdControl.setValue('');
    this.productSearch = '';
    this.filteredProducts = this.allProducts;
    this.showDropdown = false;
  }

  openDropdown() {
    this.filteredProducts = this.productSearch
      ? this.filteredProducts
      : this.allProducts;
    this.showDropdown = true;
  }

  /** Close dropdown when clicking outside */
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.el.nativeElement.contains(event.target)) {
      this.showDropdown = false;
    }
  }

  getInitials(name: string): string {
    return name.split(' ').slice(0, 2).map((w: string) => w[0]).join('').toUpperCase();
  }

  // ---- Tab / Status helpers ----
  setTab(tab: 'Pricing' | 'Inventory' | 'Approval') { this.activeTab = tab; }

  isStatusPast(status: ApprovalStatus): boolean {
    const currentIdx = this.statusOrder.indexOf(this.currentApprovalStatus);
    const stepIdx    = this.statusOrder.indexOf(status);
    return stepIdx < currentIdx;
  }

  getDiscountPercent(): number {
    const mrp  = +this.pricingForm.get('mrp')?.value;
    const sale = +this.pricingForm.get('salePrice')?.value;
    if (!mrp || !sale || sale >= mrp) return 0;
    return Math.round(((mrp - sale) / mrp) * 100);
  }

  getSavingsAmount(): string {
    const mrp  = +this.pricingForm.get('mrp')?.value;
    const sale = +this.pricingForm.get('salePrice')?.value;
    if (!mrp || !sale) return '0';
    return (mrp - sale).toFixed(2);
  }

  // ---- Validators ----
  private priceValidator(group: AbstractControl): ValidationErrors | null {
    const mrp  = group.get('mrp')?.value;
    const sale = group.get('salePrice')?.value;
    if (mrp !== null && sale !== null && sale > mrp) return { salePriceGreater: true };
    return null;
  }

  private remarksValidator(group: AbstractControl): ValidationErrors | null {
    const status  = group.get('newStatus')?.value;
    const remarks = group.get('remarks')?.value;
    if (status === 'Rejected' && (!remarks || remarks.trim() === '')) return { remarksRequired: true };
    return null;
  }

  // ---- Actions ----
  savePricing() {
    if (this.pricingForm.invalid || !this.productIdControl.value) return;
    this.isSavingPricing = true;
    this.workflowService.updatePricing(this.productIdControl.value!, this.pricingForm.value).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message ?? 'Pricing updated successfully');
        this.fetchReviewData(this.productIdControl.value!);
        this.isSavingPricing = false;
      },
      error: (err: any) => {
        this.notificationService.showError(err.error?.message ?? 'Operation failed');
        this.isSavingPricing = false;
      }
    });
  }

  saveInventory() {
    if (this.inventoryForm.invalid || !this.productIdControl.value) return;
    this.isSavingInventory = true;
    this.workflowService.updateInventory(this.productIdControl.value!, this.inventoryForm.value).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message ?? 'Inventory updated successfully');
        this.fetchReviewData(this.productIdControl.value!);
        this.isSavingInventory = false;
      },
      error: (err: any) => {
        this.notificationService.showError(err.error?.message ?? 'Operation failed');
        this.isSavingInventory = false;
      }
    });
  }

  submitForReview() {
    if (!this.productIdControl.value) return;
    this.isSubmitting = true;
    this.workflowService.submitForReview(this.productIdControl.value!).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message ?? 'Submitted for review');
        this.currentApprovalStatus = 'Pending';
        this.isSubmitting = false;
      },
      error: (err: any) => {
        this.notificationService.showError(err.error?.message ?? 'Operation failed');
        this.isSubmitting = false;
      }
    });
  }

  approveProduct() {
    this.performStatusUpdate('Approved');
  }

  rejectProduct() {
    this.performStatusUpdate('Rejected');
  }

  private performStatusUpdate(status: ApprovalStatus) {
    if (!this.productIdControl.value) return;
    this.isUpdatingStatus = true;
    const request = {
      newStatus: status,
      remarks: this.statusForm.get('remarks')?.value || ''
    };

    this.workflowService.updateStatus(this.productIdControl.value!, request).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message ?? `Product ${status}`);
        this.currentApprovalStatus = status;
        this.isUpdatingStatus = false;
        this.statusForm.patchValue({ remarks: '' });
      },
      error: (err: any) => {
        this.notificationService.showError(err.error?.message ?? 'Operation failed');
        this.isUpdatingStatus = false;
      }
    });
  }

  updateStatus() {
    if (this.statusForm.invalid || !this.productIdControl.value) return;
    this.isUpdatingStatus = true;
    const newStatus = this.statusForm.value.newStatus;
    this.workflowService.updateStatus(this.productIdControl.value!, this.statusForm.value).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message ?? 'Status updated successfully');
        this.currentApprovalStatus = newStatus;
        this.statusForm.reset({ newStatus: 'Pending' });
        this.isUpdatingStatus = false;
      },
      error: (err: any) => {
        this.notificationService.showError(err.error?.message ?? 'Operation failed');
        this.isUpdatingStatus = false;
      }
    });
  }
}
