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
