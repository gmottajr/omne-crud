export interface Product {
  id: number;
  sku: string;
  name: string;
  price: number;
  description: string;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateProductRequest {
  sku: string;
  name: string;
  price: number;
  description: string;
}

export interface UpdateProductRequest {
  name: string;
  price: number;
  description: string;
}

export interface ApplicationResponse<T = unknown> {
  success: boolean;
  data?: T | null;
  errorCode?: string | null;
  errorMessage?: string | null;
}

export class ApiError extends Error {
  readonly status?: number;
  readonly errorCode?: string | null;
  readonly isNetworkError: boolean;
  readonly cause?: unknown;

  constructor(
    message: string,
    options: {
      status?: number;
      errorCode?: string | null;
      isNetworkError?: boolean;
      cause?: unknown;
    } = {}
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = options.status;
    this.errorCode = options.errorCode;
    this.isNetworkError = options.isNetworkError ?? false;
    this.cause = options.cause;
  }
}
