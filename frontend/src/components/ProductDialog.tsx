import { useEffect, type MouseEvent } from 'react';
import type { CreateProductRequest, Product, UpdateProductRequest } from '../types/product';
import { ProductForm } from './ProductForm';

interface ProductDialogProps {
  mode: 'create' | 'edit';
  product?: Product;
  disabled: boolean;
  error?: string | null;
  onCreate: (request: CreateProductRequest) => void | Promise<void>;
  onUpdate: (request: UpdateProductRequest) => void | Promise<void>;
  onClose: () => void;
}

export function ProductDialog({
  mode,
  product,
  disabled,
  error,
  onCreate,
  onUpdate,
  onClose
}: ProductDialogProps) {
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && !disabled) {
        onClose();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    document.body.classList.add('modal-open');

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.body.classList.remove('modal-open');
    };
  }, [disabled, onClose]);

  const handleBackdropClick = (event: MouseEvent<HTMLDivElement>) => {
    if (event.target === event.currentTarget && !disabled) {
      onClose();
    }
  };

  return (
    <div className="modal-backdrop" onMouseDown={handleBackdropClick}>
      <section
        className="product-dialog"
        role="dialog"
        aria-modal="true"
        aria-label={mode === 'create' ? 'Add new product' : `Edit ${product?.name ?? 'product'}`}
      >
        <button
          className="dialog-close"
          type="button"
          aria-label="Close product form"
          onClick={onClose}
          disabled={disabled}
        >
          ×
        </button>

        {error && (
          <div className="message error-message dialog-error" role="alert" aria-live="polite">
            {error}
          </div>
        )}

        {mode === 'create' ? (
          <ProductForm
            mode="create"
            onSubmit={onCreate}
            onCancel={onClose}
            disabled={disabled}
          />
        ) : product ? (
          <ProductForm
            mode="edit"
            product={product}
            onSubmit={onUpdate}
            onCancel={onClose}
            disabled={disabled}
          />
        ) : null}
      </section>
    </div>
  );
}
