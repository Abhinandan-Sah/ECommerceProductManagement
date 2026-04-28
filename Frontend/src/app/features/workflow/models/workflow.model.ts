export type ApprovalStatus =
  | 'Draft'
  | 'InEnrichment'
  | 'ReadyForReview'
  | 'Approved'
  | 'Published'
  | 'Rejected'
  | 'Archived';

export interface UpdatePricingRequest {
  MRP: number;
  SalePrice: number;
  GSTPercent: number;
}

export interface UpdateInventoryRequest {
  warehouseLocation: string;
  availableQty: number;
  reorderLevel: number;
}

export interface UpdateStatusRequest {
  newStatus: ApprovalStatus;
  remarks?: string | null;
}

export interface WorkflowMessageResponse {
  message: string;
}
