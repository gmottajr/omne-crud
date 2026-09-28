import type { Product } from '../types/product';

interface ProductListProps {
  products: Product[];
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

function formatDate(value?: string | null): string {
  if (!value) {
    return '—';
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '—' : dateFormatter.format(date);
}

export function ProductList({ products, deletingIds, onEdit, onDelete }: ProductListProps) {
  if (products.length === 0) {
    return (
      <div className="empty-state" role="status">
        <h2>No products registered</h2>
        <p>Add the first product using the form.</p>
      </div>
    );
  }

  return (
    <div className="product-list" aria-label="Product list">
      {products.map((product) => {
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
  );
}
