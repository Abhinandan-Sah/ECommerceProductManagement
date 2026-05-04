import { Component, Input, OnInit, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MediaAssetService } from '../../services/media-asset.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { MediaAssetResponse } from '../../models/media-asset.model';
import { AuthStateService } from '../../../../core/state/auth-state.service';

@Component({
  selector: 'app-media-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './media-management.component.html',
  styleUrls: ['./media-management.component.css']
})
export class MediaManagementComponent implements OnInit {
  @Input() productId!: string;

  private fb = inject(FormBuilder);
  private mediaService = inject(MediaAssetService);
  private notify = inject(NotificationService);
  private auth = inject(AuthStateService);

  mediaAssets: MediaAssetResponse[] = [];
  isLoading = false;
  isAdding = false;
  isSaving = false;
  canManageMedia = false;

  addForm!: FormGroup;

  constructor() {
    effect(() => {
      const role = this.auth.userRole();
      this.canManageMedia = ['Admin', 'ProductManager', 'ContentExecutive'].includes(role || '');
    });
  }

  ngOnInit(): void {
    this.initForm();
    if (this.productId) {
      this.loadMedia();
    }
  }

  initForm(): void {
    this.addForm = this.fb.group({
      url: ['', [Validators.required, Validators.maxLength(500), Validators.pattern(/^https?:\/\/.+/)]],
      sortOrder: [0, [Validators.required, Validators.min(0)]],
      altText: ['', [Validators.maxLength(200)]]
    });
  }

  loadMedia(): void {
    this.isLoading = true;
    this.mediaService.getMediaByProduct(this.productId).subscribe({
      next: (data) => {
        this.mediaAssets = data.sort((a, b) => a.sortOrder - b.sortOrder);
        this.isLoading = false;
      },
      error: () => {
        this.notify.showError('Failed to load media assets');
        this.isLoading = false;
      }
    });
  }

  startAdd(): void {
    this.isAdding = true;
    this.addForm.reset({ sortOrder: this.mediaAssets.length });
  }

  cancelAdd(): void {
    this.isAdding = false;
    this.addForm.reset();
  }

  submitAdd(): void {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.mediaService.createMedia(this.productId, this.addForm.value).subscribe({
      next: () => {
        this.notify.showSuccess('Media asset added successfully');
        this.isAdding = false;
        this.addForm.reset();
        this.loadMedia();
      },
      error: (err) => {
        this.notify.showError(err.error?.message || 'Failed to add media asset');
        this.isSaving = false;
      }
    });
  }

  deleteMedia(mediaId: string): void {
    if (!confirm('Are you sure you want to delete this media asset?')) {
      return;
    }

    this.isLoading = true;
    this.mediaService.deleteMedia(this.productId, mediaId).subscribe({
      next: () => {
        this.notify.showSuccess('Media asset deleted successfully');
        this.loadMedia();
      },
      error: () => {
        this.notify.showError('Failed to delete media asset');
        this.isLoading = false;
      }
    });
  }

  isInvalid(controlName: string): boolean {
    const control = this.addForm.get(controlName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }
}
