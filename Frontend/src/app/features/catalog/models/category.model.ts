export interface CategoryResponse {
  id: string;
  name: string;
  parentCategoryId?: string;
  parentCategoryName: string;
}

export interface CreateCategory {
  name: string;
  parentCategoryId?: string;
}
