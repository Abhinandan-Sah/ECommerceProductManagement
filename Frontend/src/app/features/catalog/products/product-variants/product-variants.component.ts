import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';

import { CatalogService } from '../../services/catalog.service';
import { WorkflowService } from '../../../workflow/services/workflow.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ApprovalStatus } from '../../../workflow/models/workflow.model';
import { selectUserRole } from '../../../../store/auth/auth.selectors';

@Component({
  selector: 'app-product-variants',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './product-variants.component.html',
  styleUrls: ['./product-variants.component.css']
})
export class ProductVariantsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private catalogService = inject(CatalogService);
  private workflowService = inject(WorkflowService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private store = inject(Store);

  productId!: string;
  productName = 'Loading...';
  currentApprovalStatus: ApprovalStatus = 'Pending';
  variants: any[] = [];
  isLoading = false;
  
  // Side panel state
  isSidePanelOpen = false;
  panelMode: 'add' | 'edit' = 'add';
  editingVariantId: string | null = null;
  variantForm!: FormGroup;

  // RBAC
  canAddVariants = false;
  canEditVariants = false;
  canDeleteVariants = false;

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('productId')!;
    
    this.store.select(selectUserRole).subscribe(role => {
      this.canAddVariants = ['Admin', 'ProductManager', 'ContentExecutive'].includes(role || '');
      this.canEditVariants = ['Admin', 'ProductManager'].includes(role || '');
      this.canDeleteVariants = role === 'Admin';
    });

    this.initForm();
    this.loadProductDetails();
    this.loadApprovalStatus();
    this.loadVariants();
  }

  initForm(): void {
    this.variantForm = this.fb.group({
      color: ['', [Validators.required, Validators.maxLength(100)]],
      size: ['', [Validators.required, Validators.maxLength(50)]],
      barcode: ['', [Validators.required, Validators.maxLength(100)]]
    });
  }

  loadProductDetails(): void {
    this.catalogService.getProductById(this.productId).subscribe({
      next: (data) => this.productName = data.name,
      error: () => this.productName = 'Product Details Unavailable'
    });
  }

  loadApprovalStatus(): void {
    this.workflowService.getApprovalStatus(this.productId).subscribe({
      next: (data) => {
        this.currentApprovalStatus = data.status;
      },
      error: () => {
        this.currentApprovalStatus = 'Pending';
      }
    });
  }

  loadVariants(): void {
    this.isLoading = true;
    this.catalogService.getVariants(this.productId).subscribe({
      next: (data) => {
        this.variants = data;
        this.isLoading = false;
      },
      error: () => {
        this.notify.showError('Failed to load variants');
        this.isLoading = false;
      }
    });
  }

  openPanel(mode: 'add' | 'edit', variant?: any): void {
    this.panelMode = mode;
    this.isSidePanelOpen = true;
    
    if (mode === 'edit' && variant) {
      this.editingVariantId = variant.id;
      this.variantForm.patchValue({
        color: variant.color,
        size: variant.size,
        barcode: variant.barcode
      });
    } else {
      this.editingVariantId = null;
      this.variantForm.reset();
    }
  }

  closePanel(): void {
    this.isSidePanelOpen = false;
    this.variantForm.reset();
    this.editingVariantId = null;
  }

  submitVariant(): void {
    if (this.variantForm.invalid) {
      this.variantForm.markAllAsTouched();
      return;
    }
    
    this.isLoading = true;
    
    if (this.panelMode === 'add') {
      this.catalogService.createVariant(this.productId, this.variantForm.value).subscribe({
        next: () => {
          this.notify.showSuccess('Variant added successfully');
          this.closePanel();
          this.loadVariants();
        },
        error: () => {
          this.notify.showError('Failed to add variant');
          this.isLoading = false;
        }
      });
    } else if (this.panelMode === 'edit' && this.editingVariantId) {
      this.catalogService.updateVariant(this.productId, this.editingVariantId, this.variantForm.value).subscribe({
        next: () => {
          this.notify.showSuccess('Variant updated successfully');
          this.closePanel();
          this.loadVariants();
          this.loadApprovalStatus();
        },
        error: () => {
          this.notify.showError('Failed to update variant');
          this.isLoading = false;
        }
      });
    }
  }

  deleteVariant(variantId: string): void {
    if (confirm('Are you sure you want to delete this variant?')) {
      this.isLoading = true;
      this.catalogService.deleteVariant(this.productId, variantId).subscribe({
        next: () => {
          this.notify.showSuccess('Variant deleted successfully');
          this.loadVariants();
        },
        error: () => {
          this.notify.showError('Failed to delete variant');
          this.isLoading = false;
        }
      });
    }
  }
}
