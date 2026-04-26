import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';

import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { CategoryResponse } from '../../models/category.model';
import { selectUserRole } from '../../../../store/auth/auth.selectors';

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

  // Edit state
  editingCategoryId: string | null = null;
  editForm!: FormGroup;

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
    this.addForm.reset({ parentCategoryId: '' });
  }

  cancelAdd(): void {
    this.isAdding = false;
  }

  submitAdd(): void {
    if (this.addForm.invalid) return;
    
    this.isLoading = true;
    const dto = this.addForm.value;
    if (!dto.parentCategoryId) delete dto.parentCategoryId;

    this.categoryService.createCategory(dto).subscribe({
      next: () => {
        this.notify.showSuccess('Category added');
        this.isAdding = false;
        this.loadCategories();
      },
      error: () => {
        this.notify.showError('Failed to add category');
        this.isLoading = false;
      }
    });
  }

  startEdit(category: CategoryResponse): void {
    this.editingCategoryId = category.id;
    this.editForm.patchValue({
      name: category.name,
      parentCategoryId: category.parentCategoryId || ''
    });
  }

  cancelEdit(): void {
    this.editingCategoryId = null;
  }

  submitEdit(id: string): void {
    if (this.editForm.invalid) return;

    this.isLoading = true;
    const dto = this.editForm.value;
    if (!dto.parentCategoryId) delete dto.parentCategoryId;

    this.categoryService.updateCategory(id, dto).subscribe({
      next: () => {
        this.notify.showSuccess('Category updated');
        this.editingCategoryId = null;
        this.loadCategories();
      },
      error: () => {
        this.notify.showError('Failed to update category');
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
