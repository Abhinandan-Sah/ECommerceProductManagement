export interface MediaAssetResponse {
  id: string;
  productId: string;
  url: string;
  sortOrder: number;
  altText: string;
}

export interface CreateMediaAsset {
  url: string;        // Required, max 500 chars, valid URL format
  sortOrder: number;  // Non-negative integer
  altText: string;    // Max 200 chars
}
