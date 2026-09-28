import { useCallback, useEffect, useState } from 'react';
import './App.css';
import { productsApi } from './api/productsApi';
import { ProductFilter } from './components/ProductFilter';
import { ProductForm } from './components/ProductForm';
import { ProductList } from './components/ProductList';
import {
  ApiError,
  type CreateProductRequest,
  type Product,
  type ProductFilters,
  type UpdateProductRequest
} from './types/product';

type LoadMode = 'initial' | 'refresh';

function errorMessage(error: unknown, fallback: string): string {
  return error instanceof ApiError ? error.message : fallback;
}

function hasFilters(filters: ProductFilters): boolean {
  return Object.values(filters).some((value) => value !== undefined && value !== '');
}

function App() {
  const [products, setProducts] = useState<Product[]>([]);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [isInitialLoading, setIsInitialLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [deletingIds, setDeletingIds] = useState<ReadonlySet<number>>(() => new Set());
  const [loadError, setLoadError] = useState<string | null>(null);
  const [mutationError, setMutationError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [createFormVersion, setCreateFormVersion] = useState(0);
  const [activeFilters, setActiveFilters] = useState<ProductFilters>({});

  const loadProducts = useCallback(async (
    mode: LoadMode,
    filters: ProductFilters = {}
  ): Promise<boolean> => {
    if (mode === 'initial') {
      setIsInitialLoading(true);
    } else {
      setIsRefreshing(true);
    }

    setLoadError(null);

    try {
      const data = hasFilters(filters)
        ? await productsApi.filterProducts(filters)
        : await productsApi.getProducts();
      setProducts(data);
      return true;
    } catch (error) {
      setLoadError(errorMessage(error, 'Unable to load products.'));
      return false;
    } finally {
      if (mode === 'initial') {
        setIsInitialLoading(false);
      } else {
        setIsRefreshing(false);
      }
    }
  }, []);

  useEffect(() => {
    void loadProducts('initial', {});
  }, [loadProducts]);

  const handleApplyFilters = async (filters: ProductFilters) => {
    setActiveFilters(filters);
    await loadProducts('refresh', filters);
  };

  const handleClearFilters = async () => {
    setActiveFilters({});
    await loadProducts('refresh', {});
  };

  const handleCreate = async (request: CreateProductRequest) => {
    setIsSaving(true);
    setMutationError(null);
    setNotice(null);

    try {
      await productsApi.createProduct(request);
      setCreateFormVersion((version) => version + 1);
      setNotice('Product created successfully.');
      await loadProducts('refresh', activeFilters);
    } catch (error) {
      setMutationError(errorMessage(error, 'Unable to create the product.'));
    } finally {
      setIsSaving(false);
    }
  };

  const handleUpdate = async (request: UpdateProductRequest) => {
    if (!selectedProduct) {
      return;
    }

    setIsSaving(true);
    setMutationError(null);
    setNotice(null);

    try {
      await productsApi.updateProduct(selectedProduct.id, request);
      setSelectedProduct(null);
      setNotice('Product updated successfully.');
      await loadProducts('refresh', activeFilters);
    } catch (error) {
      setMutationError(errorMessage(error, 'Unable to update the product.'));
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (product: Product) => {
    const confirmed = window.confirm(
      `Delete product "${product.name}" (${product.sku})? This action cannot be undone.`
    );

    if (!confirmed) {
      return;
    }

    setDeletingIds((current) => new Set(current).add(product.id));
    setMutationError(null);
    setNotice(null);

    try {
      await productsApi.deleteProduct(product.id);
      setProducts((current) => current.filter((item) => item.id !== product.id));
      setSelectedProduct((current) => current?.id === product.id ? null : current);
      setNotice('Product deleted successfully.');
    } catch (error) {
      setMutationError(errorMessage(error, 'Unable to delete the product.'));
    } finally {
      setDeletingIds((current) => {
        const next = new Set(current);
        next.delete(product.id);
        return next;
      });
    }
  };

  const handleEdit = (product: Product) => {
    setSelectedProduct(product);
    setMutationError(null);
    setNotice(null);
  };

  const handleCancelEdit = () => {
    setSelectedProduct(null);
    setMutationError(null);
  };

  return (
    <div className="app-container">
      <header className="app-header">
        <p className="eyebrow">Omne CRUD Demo</p>
        <h1 className="app-title">Products</h1>
        <p className="app-subtitle">Manage the products registered in the application.</p>
      </header>

      <main className="main-content">
        {(mutationError || notice) && (
          <div
            className={mutationError ? 'message error-message' : 'message notice-message'}
            role={mutationError ? 'alert' : 'status'}
            aria-live="polite"
          >
            {mutationError ?? notice}
          </div>
        )}

        <div className="workspace-grid">
          <section className="form-section" aria-label="Product form">
            <div className="card form-card">
              {selectedProduct ? (
                <ProductForm
                  key={`edit-${selectedProduct.id}`}
                  mode="edit"
                  product={selectedProduct}
                  onSubmit={handleUpdate}
                  onCancel={handleCancelEdit}
                  disabled={isSaving}
                />
              ) : (
                <ProductForm
                  key={`create-${createFormVersion}`}
                  mode="create"
                  onSubmit={handleCreate}
                  disabled={isSaving}
                />
              )}
            </div>
          </section>

          <section className="products-section" aria-labelledby="products-heading">
            <div className="card">
              <div className="section-header">
                <div>
                  <p className="eyebrow">Catalog</p>
                  <h2 id="products-heading" className="section-title">Registered products</h2>
                  <p className="section-description">
                    {products.length === 1 ? '1 product found.' : `${products.length} products found.`}
                  </p>
                </div>
                <button
                  className="refresh-button"
                  onClick={() => void loadProducts('refresh', activeFilters)}
                  disabled={isInitialLoading || isRefreshing}
                  type="button"
                >
                  {isRefreshing ? 'Refreshing...' : 'Refresh list'}
                </button>
              </div>

              {loadError && (
                <div className="message error-message" role="alert" aria-live="polite">
                  <span>{loadError}</span>
                  <button type="button" onClick={() => void loadProducts('refresh', activeFilters)}>
                    Try again
                  </button>
                </div>
              )}

              <ProductFilter
                disabled={isInitialLoading || isRefreshing}
                onApply={(filters) => void handleApplyFilters(filters)}
                onClear={() => void handleClearFilters()}
              />

              {isInitialLoading && products.length === 0 ? (
                <div className="loading-state" role="status" aria-live="polite">
                  Loading products...
                </div>
              ) : loadError && products.length === 0 ? (
                <div className="unavailable-state" role="status">
                  The product list is currently unavailable.
                </div>
              ) : (
                <ProductList
                  products={products}
                  isFiltered={hasFilters(activeFilters)}
                  deletingIds={deletingIds}
                  onEdit={handleEdit}
                  onDelete={(product) => void handleDelete(product)}
                />
              )}
            </div>
          </section>
        </div>
      </main>

      <footer className="app-footer">
        <span>Omne CRUD Demo</span>
      </footer>
    </div>
  );
}

export default App;
