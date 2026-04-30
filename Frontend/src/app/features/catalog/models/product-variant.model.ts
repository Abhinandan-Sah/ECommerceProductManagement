export interface ProductVariantResponse {
  id: string;
  productId: string;
  color: string;
  size: string;
  barcode: string;
}

export interface CreateProductVariant {
  color: string;
  size: string;
  barcode: string;
}

export interface UpdateProductVariant {
  color: string;
  size: string;
  barcode: string;
}
