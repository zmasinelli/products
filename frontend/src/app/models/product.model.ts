export interface Product {
  id: number;
  name: string;
  description: string | null;
  price: number;
  categoryId: number;
  categoryName: string;
  stockQuantity: number;
  createdDate: string;
  isActive: boolean;
}

export interface SearchProductsResponse {
  items: Product[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateProductRequest {
  name: string;
  description?: string | null;
  price: number;
  categoryId: number;
  stockQuantity: number;
}

export interface UpdateProductRequest {
  name?: string;
  description?: string | null;
  price?: number;
  categoryId?: number;
  stockQuantity?: number;
}
