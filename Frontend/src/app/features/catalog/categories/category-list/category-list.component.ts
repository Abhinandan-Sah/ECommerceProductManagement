import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { HttpErrorResponse } from '@angular/common/http';

import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { CategoryResponse } from '../../models/category.model';
import { selectUserRole } from '../../../../store/auth/auth.selectors';
import { extractErrorMessage } from '../../../../core/utils/error-utils';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.css']
})
export class CategoryListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private categoryService = inject(CategoryService);
  private notify = inject(NotificationService);
  private store = inject(Store);

  categories: CategoryResponse[] = [];
  rootCategories: CategoryResponse[] = [];
  isLoading = false;

  // Add state
  isAdding = false;
  addForm!: FormGroup;
  addCategoryConflictError = ''; // For displaying 409 conflict error

  // Edit state
  editingCategoryId: string | null = null;
  editingCategory: CategoryResponse | null = null;
  editForm!: FormGroup;
  editCategoryConflictError = ''; // For displaying 409 conflict error

  // RBAC
  canManageCategories = false;
  canDeleteCategories = false;

  ngOnInit(): void {
    this.store.select(selectUserRole).subscribe(role => {
      this.canManageCategories = ['Admin', 'ProductManager'].includes(role || '');
      this.canDeleteCategories = role === 'Admin';
    });

    this.initForms();
    this.loadCategories();
  }

  initForms(): void {
    this.addForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      parentCategoryId: ['']
    });

    this.editForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      parentCategoryId: ['']
    });
  }

  loadCategories(): void {
    this.isLoading = true;
    this.categoryService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
        this.rootCategories = data.filter(c => !c.parentCategoryId);
        this.isLoading = false;
      },
      error: () => {
        this.notify.showError('Failed to load categories');
        this.isLoading = false;
      }
    });
  }

  startAdd(): void {
    this.isAdding = true;
    this.addCategoryConflictError = '';
    this.addForm.reset({ parentCategoryId: '' });
  }

  cancelAdd(): void {
    this.isAdding = false;
    this.addCategoryConflictError = '';
  }

  submitAdd(): void {
    if (this.addForm.invalid) return;
    
    this.isLoading = true;
    this.addCategoryConflictError = '';
    const dto = this.addForm.value;
    if (!dto.parentCategoryId) delete dto.parentCategoryId;

    this.categoryService.createCategory(dto).subscribe({
      next: () => {
        this.notify.showSuccess('Category added');
        this.isAdding = false;
        this.loadCategories();
      },
      error: (err: HttpErrorResponse) => {
        const errorMsg = extractErrorMessage(err);
        
        // Handle 409 conflict error specifically for category name field
        if (err.status === 409) {
          this.addCategoryConflictError = errorMsg;
        }
        
        this.notify.showError(errorMsg);
        this.isLoading = false;
      }
    });
  }

  startEdit(category: CategoryResponse): void {
    this.editingCategoryId = category.id;
    this.editingCategory = category;
    this.editCategoryConflictError = '';
    this.editForm.patchValue({
      name: category.name,
      parentCategoryId: category.parentCategoryId || ''
    });
  }

  cancelEdit(): void {
    this.editingCategoryId = null;
    this.editingCategory = null;
    this.editCategoryConflictError = '';
  }

  submitEdit(id: string): void {
    if (this.editForm.invalid || !this.editingCategory) return;

    this.isLoading = true;
    this.editCategoryConflictError = '';
    const formValue = this.editForm.value;
    
    // Create UpdateCategory DTO with complete entity structure
    const updateDto = {
      id: id,
      name: formValue.name,
      parentCategoryId: formValue.parentCategoryId || undefined,
      createdAt: new Date().toISOString() // Backend expects this field
    };

    this.categoryService.updateCategory(id, updateDto).subscribe({
      next: () => {
        this.notify.showSuccess('Category updated');
        this.editingCategoryId = null;
        this.editingCategory = null;
        this.loadCategories();
      },
      error: (err: HttpErrorResponse) => {
        const errorMsg = extractErrorMessage(err);
        
        // Handle 409 conflict error specifically for category name field
        if (err.status === 409) {
          this.editCategoryConflictError = errorMsg;
        }
        
        this.notify.showError(errorMsg);
        this.isLoading = false;
      }
    });
  }

  deleteCategory(id: string): void {
    if (confirm('Are you sure you want to delete this category? Recursively deletes subcategories.')) {
      this.isLoading = true;
      this.categoryService.deleteCategory(id).subscribe({
        next: () => {
          this.notify.showSuccess('Category deleted');
          this.loadCategories();
        },
        error: () => {
          this.notify.showError('Failed to delete category');
          this.isLoading = false;
        }
      });
    }
  }
}
