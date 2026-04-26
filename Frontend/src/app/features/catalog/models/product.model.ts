export interface ProductResponse {
  id: string;
  name: string;
  sku: string;
  brand: string;
  description: string;
  publishStatus: PublishStatus;
  categoryName: string;
  categoryId?: string;
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
  Published = 1,
  Archived = 2
}
