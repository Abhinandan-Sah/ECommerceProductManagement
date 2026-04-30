import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Store } from '@ngrx/store';

import { CatalogService } from '../../services/catalog.service';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { CategoryResponse } from '../../models/category.model';
import { PublishStatus } from '../../models/product.model';
import { MediaManagementComponent } from '../media-management/media-management.component';
import { extractErrorMessage } from '../../../../core/utils/error-utils';
import { selectUserRole } from '../../../../store/auth/auth.selectors';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MediaManagementComponent],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.css']
})
export class ProductFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private catalogService = inject(CatalogService);
  private categoryService = inject(CategoryService);
  private notify = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private store = inject(Store);

  form!: FormGroup;
  categories: CategoryResponse[] = [];

  isEditMode = false;
  productId: string | null = null;
  isLoading = false;
  isSaving = false;

  skuConflictError = ''; // For displaying 409 conflict error for SKU

  publishStatusEnum = PublishStatus;

  /** Current user role read from NgRx auth state */
  userRole: string | null = null;

  /**
   * Status values that only an Admin is allowed to set.
   * A ProductManager must not see these as selectable options.
   */
  readonly adminOnlyStatuses: PublishStatus[] = [
    PublishStatus.Approved,
    PublishStatus.Published,
    PublishStatus.Archived
  ];

  /** True when the currently-loaded status is Admin-only AND the viewer is a ProductManager. */
  get isStatusLockedForProductManager(): boolean {
    if (this.userRole !== 'ProductManager') return false;
    const currentStatus: PublishStatus = this.form?.get('publishStatus')?.value;
    return this.adminOnlyStatuses.includes(currentStatus);
  }

  /** Human-readable label for the current locked status */
  get currentStatusLabel(): string {
    const s: PublishStatus = this.form?.get('publishStatus')?.value;
    switch (s) {
      case PublishStatus.Approved:  return 'Approved';
      case PublishStatus.Published: return 'Published';
      case PublishStatus.Rejected:  return 'Rejected';
      case PublishStatus.Archived:  return 'Archived';
      default: return '';
    }
  }

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.productId;

    // Read the current user role from the NgRx store
    this.store.select(selectUserRole).subscribe(role => {
      this.userRole = role;
    });

    this.loadCategories();
    this.initForm();

    if (this.isEditMode) {
      this.loadProduct(this.productId!);
    }
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data) => this.categories = data,
      error: () => this.notify.showError('Failed to load categories')
    });
  }

  initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      brand: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(500)]],
      categoryId: ['', [Validators.required]],
      publishStatus: [PublishStatus.Draft]
    });
  }

  loadProduct(id: string): void {
    this.isLoading = true;
    this.catalogService.getProductById(id).subscribe({
      next: (product) => {
        this.form.patchValue({
          name: product.name,
          brand: product.brand,
          description: product.description,
          categoryId: product.categoryId || '',
          publishStatus: product.publishStatus
        });
        this.isLoading = false;
      },
      error: () => {
        this.notify.showError('Failed to load product details');
        this.isLoading = false;
        this.router.navigate(['/catalog/products']);
      }
    });
  }

  get descriptionLength(): number {
    return this.form.get('description')?.value?.length || 0;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.skuConflictError = '';
    const dto = this.form.value;

    if (this.isEditMode) {
      this.catalogService.updateProduct(this.productId!, dto).subscribe({
        next: () => {
          this.isSaving = false;
          this.notify.showSuccess('Product updated successfully');
          this.router.navigate(['/catalog/products']);
        },
        error: (err: HttpErrorResponse) => {
          this.isSaving = false;
          const errorMsg = extractErrorMessage(err);

          // Handle 409 conflict error specifically for SKU field
          if (err.status === 409) {
            this.skuConflictError = errorMsg;
          }

          this.notify.showError(errorMsg);
        }
      });
    } else {
      this.catalogService.createProduct(dto).subscribe({
        next: (response) => {
          this.isSaving = false;
          this.notify.showSuccess(`Product created successfully! SKU: ${response.sku}`);
          this.router.navigate(['/catalog/products']);
        },
        error: (err: HttpErrorResponse) => {
          this.isSaving = false;
          const errorMsg = extractErrorMessage(err);

          // Handle 409 conflict error specifically for SKU field
          if (err.status === 409) {
            this.skuConflictError = errorMsg;
          }

          this.notify.showError(errorMsg);
        }
      });
    }
  }

  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }
}
