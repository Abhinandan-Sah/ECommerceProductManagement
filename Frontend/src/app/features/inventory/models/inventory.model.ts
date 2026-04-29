export interface ProductInventory {
  productId: string;
  productName: string;
  sku: string;
  warehouseLocation: string;
  availableQty: number;
  reorderLevel: number;
}

export interface InventoryResponse {
  productId: string;
  warehouseLocation: string;
  availableQty: number;
  reorderLevel: number;
}
