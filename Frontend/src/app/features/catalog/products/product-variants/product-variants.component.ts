import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';

import { CatalogService } from '../../services/catalog.service';
import { WorkflowService } from '../../../workflow/services/workflow.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { WorkflowStateTimelineComponent } from '../../../workflow/components/workflow-state-timeline/workflow-state-timeline.component';
import { ProductResponse } from '../../models/product.model';
import { ProductVariantResponse } from '../../models/product-variant.model';
import { ApprovalStatus } from '../../../workflow/models/workflow.model';
import { selectUserRole } from '../../../../store/auth/auth.selectors';

@Component({
  selector: 'app-product-variants',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, WorkflowStateTimelineComponent],
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
  
  // Inline edit state
  editingVariantId: string | null = null;
  editForm!: FormGroup;

  // Add new variant state
  isAdding = false;
  addForm!: FormGroup;

  // RBAC
  canManageVariants = false;
  canDeleteVariants = false;

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('productId')!;
    
    this.store.select(selectUserRole).subscribe(role => {
      this.canManageVariants = ['Admin', 'ProductManager', 'ContentExecutive'].includes(role || '');
      this.canDeleteVariants = role === 'Admin';
    });

    this.initForms();
    this.loadProductDetails();
    this.loadApprovalStatus();
    this.loadVariants();
  }

  initForms(): void {
    this.addForm = this.fb.group({
      color: ['', [Validators.required, Validators.maxLength(50)]],
      size: ['', [Validators.required, Validators.maxLength(50)]]
    });

    this.editForm = this.fb.group({
      color: ['', [Validators.required, Validators.maxLength(50)]],
      size: ['', [Validators.required, Validators.maxLength(50)]]
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
        // Default to Pending if no approval status found
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

  startAdd(): void {
    this.isAdding = true;
    this.addForm.reset();
  }

  cancelAdd(): void {
    this.isAdding = false;
  }

  submitAdd(): void {
    if (this.addForm.invalid) return;
    
    this.isLoading = true;
    this.catalogService.createVariant(this.productId, this.addForm.value).subscribe({
      next: () => {
        this.notify.showSuccess('Variant added');
        this.isAdding = false;
        this.loadVariants();
      },
      error: () => {
        this.notify.showError('Failed to add variant');
        this.isLoading = false;
      }
    });
  }

  startEdit(variant: any): void {
    this.editingVariantId = variant.id;
    this.editForm.patchValue({
      color: variant.color,
      size: variant.size
    });
  }

  cancelEdit(): void {
    this.editingVariantId = null;
  }

  submitEdit(variantId: string): void {
    if (this.editForm.invalid) return;

    this.isLoading = true;
    this.catalogService.updateVariant(this.productId, variantId, this.editForm.value).subscribe({
      next: () => {
        this.notify.showSuccess('Variant updated');
        this.editingVariantId = null;
        this.loadVariants();
        this.loadApprovalStatus(); // Reload approval status in case it changed
      },
      error: () => {
        this.notify.showError('Failed to update variant');
        this.isLoading = false;
      }
    });
  }

  deleteVariant(variantId: string): void {
    if (confirm('Are you sure you want to delete this variant?')) {
      this.isLoading = true;
      this.catalogService.deleteVariant(this.productId, variantId).subscribe({
        next: () => {
          this.notify.showSuccess('Variant deleted');
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
