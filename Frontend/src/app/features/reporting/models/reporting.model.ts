export interface DashboardKpi {
  id: string;
  snapshotDate: string;
  totalProducts: number;
  publishedProducts: number;
  pendingApprovals: number;
  rejectedProducts: number;
  totalUsers: number;
}

export interface ProductReport {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  status: string;
  publishedAt: string | null;
  createdByUserId: string;
  categoryName: string;
}

export interface ProductReportFilter {
  pageNumber: number;
  pageSize: number;
  category?: string;
  status?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
