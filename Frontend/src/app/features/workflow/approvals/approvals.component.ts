import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CatalogService } from '../../catalog/services/catalog.service';
import { ProductResponse } from '../../catalog/models/product.model';
import { WorkflowService } from '../services/workflow.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-approvals',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './approvals.component.html',
  styleUrls: ['./approvals.component.css']
})
export class ApprovalsComponent implements OnInit {
  private catalogService = inject(CatalogService);
  private workflowService = inject(WorkflowService);
  private router = inject(Router);

  pendingProducts: ProductResponse[] = [];
  isLoading = true;

  ngOnInit() {
    this.loadPendingApprovals();
  }

  loadPendingApprovals() {
    this.isLoading = true;
    forkJoin({
      products: this.catalogService.getProducts(),
      pendingApprovals: this.workflowService.getPendingApprovals()
    }).subscribe({
      next: ({ products, pendingApprovals }) => {
        const pendingIds = new Set(pendingApprovals.filter((a: any) => a.status === 'Pending').map((a: any) => a.productId));
        this.pendingProducts = products.filter(p => pendingIds.has(p.id));
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  reviewProduct(productId: string) {
    this.router.navigate(['/workflow'], { queryParams: { productId, tab: 'Approval' } });
  }

  getInitials(name: string): string {
    if (!name) return 'PR';
    return name.split(' ').slice(0, 2).map((w: string) => w[0]).join('').toUpperCase();
  }
}
