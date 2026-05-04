import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CatalogService } from '../../catalog/services/catalog.service';
import { WorkflowService } from '../../workflow/services/workflow.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProductInventory } from '../models/inventory.model';

@Component({
  selector: 'app-inventory-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './inventory-dashboard.component.html',
  styleUrls: ['./inventory-dashboard.component.css']
})
export class InventoryDashboardComponent implements OnInit {
  private catalogService = inject(CatalogService);
  private workflowService = inject(WorkflowService);
  private notify = inject(NotificationService);

  inventoryData: ProductInventory[] = [];
  filteredData: ProductInventory[] = [];
  isLoading = false;

  // Filter control
  showReorderOnly = new FormControl(false);

  ngOnInit(): void {
    this.loadInventoryData();
    
    // Listen to filter changes
    this.showReorderOnly.valueChanges.subscribe(() => {
      this.applyFilter();
    });
  }

  loadInventoryData(): void {
    this.isLoading = true;
    
    // First, get all products
    this.catalogService.getProducts().subscribe({
      next: (products) => {
        // For each product, fetch its inventory data
        // Use catchError to handle products without inventory data
        const inventoryRequests = products.map(product =>
          this.workflowService.getInventory(product.id, false).pipe(
            catchError(() => of(null)) // Return null if inventory doesn't exist
          )
        );

        forkJoin(inventoryRequests).subscribe({
          next: (inventories) => {
            // Combine product and inventory data
            this.inventoryData = products.map((product, index) => ({
              productId: product.id,
              productName: product.name,
              sku: product.sku,
              warehouseLocation: inventories[index]?.warehouseLocation || 'N/A',
              availableQty: inventories[index]?.availableQty || 0,
              reorderLevel: inventories[index]?.reorderLevel || 0
            }));

            this.applyFilter();
            this.isLoading = false;
          },
          error: () => {
            this.notify.showError('Failed to load inventory data');
            this.isLoading = false;
          }
        });
      },
      error: () => {
        this.notify.showError('Failed to load products');
        this.isLoading = false;
      }
    });
  }

  applyFilter(): void {
    if (this.showReorderOnly.value) {
      // Show only products below reorder level
      this.filteredData = this.inventoryData
        .filter(item => item.availableQty < item.reorderLevel)
        .sort((a, b) => this.getStockPercentage(a) - this.getStockPercentage(b));
    } else {
      this.filteredData = [...this.inventoryData];
    }
  }

  getStockPercentage(item: ProductInventory): number {
    if (item.reorderLevel === 0) return 100;
    return (item.availableQty / item.reorderLevel) * 100;
  }

  getStockStatus(item: ProductInventory): 'critical' | 'low' | 'adequate' {
    const percentage = this.getStockPercentage(item);
    
    if (item.availableQty < item.reorderLevel) {
      return 'critical';
    } else if (percentage <= 120) { // Within 20% of reorder level
      return 'low';
    } else {
      return 'adequate';
    }
  }

  getStockStatusClass(item: ProductInventory): string {
    const status = this.getStockStatus(item);
    return {
      'critical': 'text-red-600 bg-red-50',
      'low': 'text-yellow-600 bg-yellow-50',
      'adequate': 'text-green-600 bg-green-50'
    }[status];
  }

  needsReorder(item: ProductInventory): boolean {
    return item.availableQty < item.reorderLevel;
  }
}
