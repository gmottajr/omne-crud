import { useMemo, useState } from 'react';
import type { Product } from '../types/product';

interface ProductListProps {
  products: Product[];
  isFiltered: boolean;
  deletingIds: ReadonlySet<number>;
  onEdit: (product: Product) => void;
  onDelete: (product: Product) => void;
}

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD'
});

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  dateStyle: 'short',
  timeStyle: 'short'
});

type SortOption =
  | 'name-asc'
  | 'name-desc'
  | 'sku-asc'
  | 'sku-desc'
  | 'price-asc'
  | 'price-desc'
  | 'newest'
  | 'oldest';

const textCollator = new Intl.Collator(undefined, {
  numeric: true,
  sensitivity: 'base'
});

function compareProducts(first: Product, second: Product, sortOption: SortOption): number {
  switch (sortOption) {
    case 'name-asc':
      return textCollator.compare(first.name, second.name);
    case 'name-desc':
      return textCollator.compare(second.name, first.name);
    case 'sku-asc':
      return textCollator.compare(first.sku, second.sku);
    case 'sku-desc':
      return textCollator.compare(second.sku, first.sku);
    case 'price-asc':
      return first.price - second.price;
    case 'price-desc':
      return second.price - first.price;
    case 'newest':
      return new Date(second.createdAt).getTime() - new Date(first.createdAt).getTime();
    case 'oldest':
      return new Date(first.createdAt).getTime() - new Date(second.createdAt).getTime();
  }
}

function formatDate(value?: string | null): string {
  if (!value) {
    return '—';
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '—' : dateFormatter.format(date);
}

export function ProductList({ products, isFiltered, deletingIds, onEdit, onDelete }: ProductListProps) {
  const [sortOption, setSortOption] = useState<SortOption>('name-asc');
  const sortedProducts = useMemo(
    () => [...products].sort((first, second) => compareProducts(first, second, sortOption)),
    [products, sortOption]
  );

  if (products.length === 0) {
    return (
      <div className="empty-state" role="status">
        <h2>{isFiltered ? 'No products found' : 'No products registered'}</h2>
        <p>
          {isFiltered
            ? 'Try changing or clearing the filters.'
            : 'Add the first product using the form.'}
        </p>
      </div>
    );
  }

  return (
    <>
      <div className="sort-controls">
        <label htmlFor="product-sort">Sort products</label>
        <select
          id="product-sort"
          value={sortOption}
          onChange={(event) => setSortOption(event.target.value as SortOption)}
        >
          <option value="name-asc">Name (A–Z)</option>
          <option value="name-desc">Name (Z–A)</option>
          <option value="sku-asc">SKU (ascending)</option>
          <option value="sku-desc">SKU (descending)</option>
          <option value="price-asc">Price (low to high)</option>
          <option value="price-desc">Price (high to low)</option>
          <option value="newest">Newest first</option>
          <option value="oldest">Oldest first</option>
        </select>
      </div>
      <div className="product-list" aria-label="Product list">
        {sortedProducts.map((product) => {
          const isDeleting = deletingIds.has(product.id);

          return (
            <article className="product-card" key={product.id} aria-busy={isDeleting}>
              <div className="product-card-header">
                <div>
                  <p className="product-sku">SKU: {product.sku}</p>
                  <h2>{product.name}</h2>
                </div>
                <strong className="product-price">{currencyFormatter.format(product.price)}</strong>
              </div>
              <p className="product-description">{product.description}</p>
              <dl className="product-metadata">
                <div>
                  <dt>Created at</dt>
                  <dd>{formatDate(product.createdAt)}</dd>
                </div>
                <div>
                  <dt>Updated at</dt>
                  <dd>{formatDate(product.updatedAt)}</dd>
                </div>
              </dl>
              <div className="product-actions">
                <button type="button" onClick={() => onEdit(product)} disabled={isDeleting}>
                  Edit
                </button>
                <button
                  className="danger-button"
                  type="button"
                  onClick={() => onDelete(product)}
                  disabled={isDeleting}
                  aria-label={`Delete ${product.name}`}
                >
                  {isDeleting ? 'Deleting...' : 'Delete'}
                </button>
              </div>
            </article>
          );
        })}
      </div>
    </>
  );
}
