import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

import { CatalogService } from '../../services/catalog.service';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';
import { ProductResponse, PublishStatus } from '../../models/product.model';
import { CategoryResponse } from '../../models/category.model';
import { selectUserRole } from '../../../../store/auth/auth.selectors';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, LoadingSpinnerComponent],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {
  private catalogService = inject(CatalogService);
  private categoryService = inject(CategoryService);
  private notify = inject(NotificationService);
  private router = inject(Router);
  private store = inject(Store);

  products: ProductResponse[] = [];
  categories: CategoryResponse[] = [];
  filteredProducts: ProductResponse[] = [];
  
  isLoading = false;
  publishStatusEnum = PublishStatus;
  
  // Controls
  searchControl = new FormControl('');
  categoryControl = new FormControl('');
  statusControl = new FormControl('');

  // Stats
  totalProducts = 0;
  publishedCount = 0;
  draftCount = 0;

  // RBAC
  canManageProducts = false;
  canDeleteProducts = false;

  ngOnInit(): void {
    this.store.select(selectUserRole).subscribe(role => {
      this.canManageProducts = ['Admin', 'ProductManager', 'ContentExecutive'].includes(role || '');
      this.canDeleteProducts = role === 'Admin';
    });

    this.loadCategories();
    this.loadProducts();

    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => this.filterProducts());

    this.categoryControl.valueChanges.subscribe(() => this.loadProducts());
    this.statusControl.valueChanges.subscribe(() => this.loadProducts());
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data) => this.categories = data,
      error: () => this.notify.showError('Failed to load categories')
    });
  }

  loadProducts(): void {
    this.isLoading = true;
    const catId = this.categoryControl.value || undefined;
    const statusVal = this.statusControl.value ? Number(this.statusControl.value) : undefined;

    this.catalogService.getProducts(catId, statusVal).subscribe({
      next: (data) => {
        this.products = data;
        this.filterProducts();
        this.calculateStats();
        this.isLoading = false;
      },
      error: (err) => {
        this.notify.showError('Failed to load products');
        this.isLoading = false;
      }
    });
  }

  filterProducts(): void {
    const term = this.searchControl.value?.toLowerCase() || '';
    if (!term) {
      this.filteredProducts = [...this.products];
      return;
    }

    this.filteredProducts = this.products.filter(p => 
      p.name.toLowerCase().includes(term) || 
      p.brand.toLowerCase().includes(term) ||
      p.sku.toLowerCase().includes(term)
    );
  }

  calculateStats(): void {
    this.totalProducts = this.products.length;
    this.publishedCount = this.products.filter(p => p.publishStatus === PublishStatus.Published).length;
    this.draftCount = this.products.filter(p => p.publishStatus === PublishStatus.Draft).length;
  }

  editProduct(id: string): void {
    this.router.navigate(['/catalog/products', id, 'edit']);
  }

  viewVariants(id: string): void {
    this.router.navigate(['/catalog/products', id, 'variants']);
  }

  deleteProduct(id: string): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.isLoading = true;
      this.catalogService.deleteProduct(id).subscribe({
        next: () => {
          this.notify.showSuccess('Product deleted successfully');
          this.loadProducts();
        },
        error: () => {
          this.notify.showError('Failed to delete product');
          this.isLoading = false;
        }
      });
    }
  }

  getStatusBadgeClass(status: PublishStatus): string {
    switch (status) {
      case PublishStatus.Published: return 'badge-published';
      case PublishStatus.Draft: return 'badge-draft';
      case PublishStatus.Archived: return 'badge-archived';
      default: return 'badge-draft';
    }
  }

  getStatusText(status: PublishStatus): string {
    switch (status) {
      case PublishStatus.Published: return 'Published';
      case PublishStatus.Draft: return 'Draft';
      case PublishStatus.Archived: return 'Archived';
      default: return 'Unknown';
    }
  }
}
