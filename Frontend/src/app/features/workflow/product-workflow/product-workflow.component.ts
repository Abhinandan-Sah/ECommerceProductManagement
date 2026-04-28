import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Subscription } from 'rxjs';
import { selectUserRole } from '../../../store/auth/auth.selectors';
import { WorkflowService } from '../services/workflow.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ApprovalStatus } from '../models/workflow.model';

@Component({
  selector: 'app-product-workflow',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-workflow.component.html',
  styleUrls: ['./product-workflow.component.css']
})
export class ProductWorkflowComponent implements OnInit, OnDestroy {
  private store = inject(Store);
  private workflowService = inject(WorkflowService);
  private notificationService = inject(NotificationService);
  private fb = inject(FormBuilder);

  userRole: string | null = null;
  private roleSub?: Subscription;

  canManagePricing = false;
  canManageInventory = false;
  canSubmit = false;
  canUpdateStatus = false;

  productIdControl = this.fb.control('', Validators.required);
  
  pricingForm: FormGroup;
  inventoryForm: FormGroup;
  statusForm: FormGroup;

  activeTab: 'Pricing' | 'Inventory' | 'Approval' = 'Approval';
  currentApprovalStatus: ApprovalStatus = 'Draft';

  isSavingPricing = false;
  isSavingInventory = false;
  isSubmitting = false;
  isUpdatingStatus = false;

  constructor() {
    this.pricingForm = this.fb.group({
      MRP: ['', [Validators.required, Validators.min(0.01)]],
      SalePrice: ['', [Validators.required, Validators.min(0.01)]],
      GSTPercent: ['', [Validators.required, Validators.min(0), Validators.max(100)]]
    }, { validators: this.priceValidator });

    this.inventoryForm = this.fb.group({
      warehouseLocation: ['', [Validators.required, Validators.maxLength(200)]],
      availableQty: ['', [Validators.required, Validators.min(0)]],
      reorderLevel: ['', [Validators.required, Validators.min(0)]]
    });

    this.statusForm = this.fb.group({
      newStatus: ['Draft', Validators.required],
      remarks: ['', [Validators.maxLength(500)]]
    }, { validators: this.remarksValidator });
  }

  ngOnInit() {
    this.roleSub = this.store.select(selectUserRole).subscribe(role => {
      this.userRole = role;
      this.canManagePricing = role === 'Admin' || role === 'ProductManager';
      this.canManageInventory = role === 'Admin' || role === 'ProductManager';
      this.canSubmit = role === 'Admin' || role === 'ProductManager' || role === 'ContentExecutive';
      this.canUpdateStatus = role === 'Admin';

      // Default active tab based on permissions
      if (this.canManagePricing) {
        this.activeTab = 'Pricing';
      } else if (this.canManageInventory) {
        this.activeTab = 'Inventory';
      } else {
        this.activeTab = 'Approval';
      }
    });
  }

  ngOnDestroy() {
    this.roleSub?.unsubscribe();
  }

  setTab(tab: 'Pricing' | 'Inventory' | 'Approval') {
    this.activeTab = tab;
  }

  private priceValidator(group: AbstractControl): ValidationErrors | null {
    const mrp = group.get('MRP')?.value;
    const salePrice = group.get('SalePrice')?.value;
    if (mrp !== null && salePrice !== null && salePrice > mrp) {
      return { salePriceGreater: true };
    }
    return null;
  }

  private remarksValidator(group: AbstractControl): ValidationErrors | null {
    const status = group.get('newStatus')?.value;
    const remarks = group.get('remarks')?.value;
    if (status === 'Rejected' && (!remarks || remarks.trim() === '')) {
      return { remarksRequired: true };
    }
    return null;
  }

  savePricing() {
    if (this.pricingForm.invalid || !this.productIdControl.value) return;

    this.isSavingPricing = true;
    const productId = this.productIdControl.value;

    this.workflowService.updatePricing(productId, this.pricingForm.value).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message);
        this.pricingForm.reset();
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
    const productId = this.productIdControl.value;

    this.workflowService.updateInventory(productId, this.inventoryForm.value).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message);
        this.inventoryForm.reset();
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
    const productId = this.productIdControl.value;

    this.workflowService.submitForReview(productId).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message);
        this.isSubmitting = false;
      },
      error: (err: any) => {
        this.notificationService.showError(err.error?.message ?? 'Operation failed');
        this.isSubmitting = false;
      }
    });
  }

  updateStatus() {
    if (this.statusForm.invalid || !this.productIdControl.value) return;

    this.isUpdatingStatus = true;
    const productId = this.productIdControl.value;
    const newStatus = this.statusForm.value.newStatus;

    this.workflowService.updateStatus(productId, this.statusForm.value).subscribe({
      next: (res: any) => {
        this.notificationService.showSuccess(res.message);
        this.currentApprovalStatus = newStatus;
        this.statusForm.reset({ newStatus: 'Draft' });
        this.isUpdatingStatus = false;
      },
      error: (err: any) => {
        this.notificationService.showError(err.error?.message ?? 'Operation failed');
        this.isUpdatingStatus = false;
      }
    });
  }
}
