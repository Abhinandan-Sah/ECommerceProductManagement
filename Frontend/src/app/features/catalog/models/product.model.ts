import { ApprovalStatus } from '../../workflow/models/workflow.model';

export interface ProductResponse {
  id: string;
  name: string;
  sku: string;
  brand: string;
  description: string;
  publishStatus: PublishStatus;
  categoryName: string;
  categoryId?: string;
  approvalStatus?: ApprovalStatus;
}

export interface CreateProduct {
  name: string;
  brand: string;
  description: string;
  categoryId: string;
}

export interface UpdateProduct {
  name: string;
  brand: string;
  description: string;
  categoryId: string;
  publishStatus: PublishStatus;
}

export enum PublishStatus {
  Draft = 0,
  InEnrichment = 1,
  ReadyForReview = 2,
  Approved = 3,
  Published = 4,
  Rejected = 5,
  Archived = 6
}
