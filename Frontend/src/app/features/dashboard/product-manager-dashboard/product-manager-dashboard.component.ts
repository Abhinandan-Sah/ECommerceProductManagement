import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CatalogService } from '../../catalog/services/catalog.service';
import { CategoryService } from '../../catalog/services/category.service';
import { ProductResponse, PublishStatus } from '../../catalog/models/product.model';
import { CategoryResponse } from '../../catalog/models/category.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-product-manager-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './product-manager-dashboard.component.html',
  styleUrls: ['./product-manager-dashboard.component.css']
})
export class ProductManagerDashboardComponent implements OnInit {
  private catalogService  = inject(CatalogService);
  private categoryService = inject(CategoryService);
  private notification    = inject(NotificationService);
  private fb              = inject(FormBuilder);
  private router          = inject(Router);

  products: ProductResponse[] = [];
  categories: CategoryResponse[] = [];
  
  totalProducts  = 0;
  publishedCount = 0;
  draftCount     = 0;

  publishStatusEnum = PublishStatus;
  quickAddForm!: FormGroup;
  isSaving = false;

  ngOnInit(): void {
    this.initForm();
    this.loadData();
  }

  initForm(): void {
    this.quickAddForm = this.fb.group({
      name: ['', Validators.required],
      brand: ['', Validators.required],
      categoryId: ['', Validators.required],
      description: [''],
      publishStatus: [PublishStatus.Draft]
    });
  }

  loadData(): void {
    this.categoryService.getCategories().subscribe(res => this.categories = res);
    this.catalogService.getProducts().subscribe(products => {
      this.products = products;
      this.totalProducts  = products.length;
      this.publishedCount = products.filter(p => p.publishStatus === PublishStatus.Published).length;
      this.draftCount     = products.filter(p => p.publishStatus === PublishStatus.Draft).length;
    });
  }

  submitQuickAdd(): void {
    if (this.quickAddForm.invalid) {
      this.quickAddForm.markAllAsTouched();
      return;
    }
    this.isSaving = true;
    this.catalogService.createProduct(this.quickAddForm.value).subscribe({
      next: () => {
        this.notification.showSuccess('Product created as Draft');
        this.quickAddForm.reset({ publishStatus: PublishStatus.Draft });
        this.isSaving = false;
        this.loadData();
      },
      error: () => {
        this.notification.showError('Failed to create product');
        this.isSaving = false;
      }
    });
  }

  getStatusBadgeClass(status: PublishStatus): string {
    switch (status) {
      case PublishStatus.Published: return 'badge-published';
      case PublishStatus.Draft: return 'badge-draft';
      case PublishStatus.Archived: return 'badge-archived';
      case PublishStatus.InEnrichment: return 'badge-draft';
      case PublishStatus.ReadyForReview: return 'badge-warning';
      case PublishStatus.Approved: return 'badge-success';
      case PublishStatus.Rejected: return 'badge-danger';
      default: return 'badge-draft';
    }
  }

  getStatusText(status: PublishStatus): string {
    switch (status) {
      case PublishStatus.Published: return 'Published';
      case PublishStatus.Draft: return 'Draft';
      case PublishStatus.Archived: return 'Archived';
      case PublishStatus.InEnrichment: return 'In Enrichment';
      case PublishStatus.ReadyForReview: return 'Ready for Review';
      case PublishStatus.Approved: return 'Approved';
      case PublishStatus.Rejected: return 'Rejected';
      default: return 'Unknown';
    }
  }
}
