import { useCallback, useEffect, useState } from 'react';
import './App.css';
import { productsApi } from './api/productsApi';
import { subscribeToProductEvents } from './api/productEvents';
import { ProductFilter } from './components/ProductFilter';
import { ProductDialog } from './components/ProductDialog';
import { ProductList } from './components/ProductList';
import { ToastContainer, type ProductToast } from './components/ToastContainer';
import { DeleteProductDialog } from './components/DeleteProductDialog';
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
  const [activeFilters, setActiveFilters] = useState<ProductFilters>({});
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [toasts, setToasts] = useState<ProductToast[]>([]);
  const [productPendingDeletion, setProductPendingDeletion] = useState<Product | null>(null);

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

  useEffect(() => subscribeToProductEvents((notification) => {
    const toastId = `${notification.id}-${Date.now()}`;
    setToasts((current) => [...current.slice(-3), { ...notification, toastId }]);

    window.setTimeout(() => {
      setToasts((current) => current.filter((toast) => toast.toastId !== toastId));
    }, 6000);
  }), []);

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
      setIsCreateDialogOpen(false);
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
    setDeletingIds((current) => new Set(current).add(product.id));
    setMutationError(null);
    setNotice(null);

    try {
      await productsApi.deleteProduct(product.id);
      setProducts((current) => current.filter((item) => item.id !== product.id));
      setSelectedProduct((current) => current?.id === product.id ? null : current);
      setProductPendingDeletion(null);
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

  const handleRequestDelete = (product: Product) => {
    setMutationError(null);
    setNotice(null);
    setProductPendingDeletion(product);
  };

  const handleCancelDelete = useCallback(() => {
    setProductPendingDeletion(null);
    setMutationError(null);
  }, []);

  const handleEdit = (product: Product) => {
    setIsCreateDialogOpen(false);
    setSelectedProduct(product);
    setMutationError(null);
    setNotice(null);
  };

  const handleCloseProductDialog = () => {
    setIsCreateDialogOpen(false);
    setSelectedProduct(null);
    setMutationError(null);
  };

  const handleOpenCreateDialog = () => {
    setSelectedProduct(null);
    setMutationError(null);
    setNotice(null);
    setIsCreateDialogOpen(true);
  };

  const isProductDialogOpen = isCreateDialogOpen || selectedProduct !== null;

  return (
    <div className="app-container">
      <header className="app-header">
        <p className="eyebrow">Omne CRUD Demo</p>
        <h1 className="app-title">Products</h1>
        <p className="app-subtitle">Manage the products registered in the application.</p>
      </header>

      <main className="main-content">
        {((mutationError && !isProductDialogOpen && !productPendingDeletion) || notice) && (
          <div
            className={mutationError && !isProductDialogOpen ? 'message error-message' : 'message notice-message'}
            role={mutationError && !isProductDialogOpen ? 'alert' : 'status'}
            aria-live="polite"
          >
            {mutationError && !isProductDialogOpen ? mutationError : notice}
          </div>
        )}

        <div className="catalog-layout">
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
                <div className="catalog-actions">
                  <button
                    className="add-product-button"
                    onClick={handleOpenCreateDialog}
                    disabled={isSaving}
                    type="button"
                  >
                    Add new product
                  </button>
                  <button
                    className="refresh-button"
                    onClick={() => void loadProducts('refresh', activeFilters)}
                    disabled={isInitialLoading || isRefreshing}
                    type="button"
                  >
                    {isRefreshing ? 'Refreshing...' : 'Refresh list'}
                  </button>
                </div>
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
                  onDelete={handleRequestDelete}
                />
              )}
            </div>
          </section>
        </div>
      </main>

      {isProductDialogOpen && (
        <ProductDialog
          mode={selectedProduct ? 'edit' : 'create'}
          product={selectedProduct ?? undefined}
          disabled={isSaving}
          error={mutationError}
          onCreate={handleCreate}
          onUpdate={handleUpdate}
          onClose={handleCloseProductDialog}
        />
      )}

      {productPendingDeletion && (
        <DeleteProductDialog
          product={productPendingDeletion}
          disabled={deletingIds.has(productPendingDeletion.id)}
          error={mutationError}
          onConfirm={() => handleDelete(productPendingDeletion)}
          onCancel={handleCancelDelete}
        />
      )}

      <ToastContainer
        toasts={toasts}
        onDismiss={(toastId) => setToasts((current) =>
          current.filter((toast) => toast.toastId !== toastId))}
      />

      <footer className="app-footer">
        <span>Omne CRUD Demo</span>
      </footer>
    </div>
  );
}

export default App;
