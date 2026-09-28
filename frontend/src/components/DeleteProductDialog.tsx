import { useEffect, useRef, type MouseEvent } from 'react';
import type { Product } from '../types/product';

interface DeleteProductDialogProps {
  product: Product;
  disabled: boolean;
  error?: string | null;
  onConfirm: () => void | Promise<void>;
  onCancel: () => void;
}

export function DeleteProductDialog({
  product,
  disabled,
  error,
  onConfirm,
  onCancel
}: DeleteProductDialogProps) {
  const cancelButtonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    cancelButtonRef.current?.focus();

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && !disabled) {
        onCancel();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    document.body.classList.add('modal-open');

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      document.body.classList.remove('modal-open');
    };
  }, [disabled, onCancel]);

  const handleBackdropClick = (event: MouseEvent<HTMLDivElement>) => {
    if (event.target === event.currentTarget && !disabled) {
      onCancel();
    }
  };

  return (
    <div className="modal-backdrop delete-modal-backdrop" onMouseDown={handleBackdropClick}>
      <section
        className="delete-dialog"
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="delete-dialog-title"
        aria-describedby="delete-dialog-description"
      >
        <div className="delete-dialog-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24" focusable="false">
            <path d="M4 7h16M9 7V4h6v3m3 0-1 13H7L6 7m4 4v5m4-5v5" />
          </svg>
        </div>

        <div className="delete-dialog-content">
          <p className="delete-dialog-eyebrow">Confirm deletion</p>
          <h2 id="delete-dialog-title">Delete this product?</h2>
          <p id="delete-dialog-description">
            This will permanently remove the product from your catalog. This action cannot be undone.
          </p>

          <div className="delete-product-summary">
            <span>{product.name}</span>
            <strong>{product.sku}</strong>
          </div>

          {error && (
            <div className="delete-dialog-error" role="alert">
              {error}
            </div>
          )}

          <div className="delete-dialog-actions">
            <button
              ref={cancelButtonRef}
              className="secondary-button"
              type="button"
              onClick={onCancel}
              disabled={disabled}
            >
              Keep product
            </button>
            <button
              className="confirm-delete-button"
              type="button"
              onClick={() => void onConfirm()}
              disabled={disabled}
            >
              {disabled ? 'Deleting...' : 'Delete product'}
            </button>
          </div>
        </div>
      </section>
    </div>
  );
}
