import { httpClient } from './httpClient';
import type {
  CreateProductRequest,
  Product,
  ProductFilters,
  UpdateProductRequest
} from '../types/product';

function filterQuery(filters: ProductFilters): string {
  const parameters = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value !== undefined && value !== '') {
      parameters.set(key, String(value));
    }
  });

  return parameters.toString();
}

export const productsApi = {
  getProducts: () => httpClient.get<Product[]>('/products'),
  filterProducts: (filters: ProductFilters) =>
    httpClient.get<Product[]>(`/products/filter?${filterQuery(filters)}`),
  getProductById: (id: number) => httpClient.get<Product>(`/products/${id}`),
  getProductBySku: (sku: string) => httpClient.get<Product>(`/products/sku/${encodeURIComponent(sku)}`),
  createProduct: (request: CreateProductRequest) => httpClient.post<CreateProductRequest, number>('/products', request),
  updateProduct: (id: number, request: UpdateProductRequest) =>
    httpClient.put<UpdateProductRequest>(`/products/${id}`, request),
  deleteProduct: (id: number) => httpClient.delete(`/products/${id}`)
};
