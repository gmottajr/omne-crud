import { httpClient } from './httpClient';
import type {
  CreateProductRequest,
  Product,
  UpdateProductRequest
} from '../types/product';

export const productsApi = {
  getProducts: () => httpClient.get<Product[]>('/products'),
  getProductById: (id: number) => httpClient.get<Product>(`/products/${id}`),
  getProductBySku: (sku: string) => httpClient.get<Product>(`/products/sku/${encodeURIComponent(sku)}`),
  createProduct: (request: CreateProductRequest) => httpClient.post<CreateProductRequest, number>('/products', request),
  updateProduct: (id: number, request: UpdateProductRequest) =>
    httpClient.put<UpdateProductRequest>(`/products/${id}`, request),
  deleteProduct: (id: number) => httpClient.delete(`/products/${id}`)
};
