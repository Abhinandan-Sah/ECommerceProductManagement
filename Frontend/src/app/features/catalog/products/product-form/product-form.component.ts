import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { CatalogService } from '../../services/catalog.service';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { CategoryResponse } from '../../models/category.model';
import { PublishStatus } from '../../models/product.model';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
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

  form!: FormGroup;
  categories: CategoryResponse[] = [];
  
  isEditMode = false;
  productId: string | null = null;
  isLoading = false;
  isSaving = false;

  publishStatusEnum = PublishStatus;

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.productId;
    
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
      publishStatus: [PublishStatus.Draft] // Only visible/used in edit mode ideally, but we'll include it here
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
    const dto = this.form.value;

    if (this.isEditMode) {
      this.catalogService.updateProduct(this.productId!, dto).subscribe({
        next: () => {
          this.notify.showSuccess('Product updated successfully');
          this.router.navigate(['/catalog/products']);
        },
        error: () => {
          this.notify.showError('Failed to update product');
          this.isSaving = false;
        }
      });
    } else {
      this.catalogService.createProduct(dto).subscribe({
        next: () => {
          this.notify.showSuccess('Product created successfully');
          this.router.navigate(['/catalog/products']);
        },
        error: () => {
          this.notify.showError('Failed to create product');
          this.isSaving = false;
        }
      });
    }
  }

  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }
}
