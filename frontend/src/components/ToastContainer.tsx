import type { ProductEventNotification } from '../api/productEvents';

export interface ProductToast extends ProductEventNotification {
  toastId: string;
}

interface ToastContainerProps {
  toasts: ProductToast[];
  onDismiss: (toastId: string) => void;
}

const operationLabels = {
  created: 'created',
  updated: 'updated',
  deleted: 'deleted'
} as const;

export function ToastContainer({ toasts, onDismiss }: ToastContainerProps) {
  return (
    <div className="toast-container" aria-live="polite" aria-label="Product notifications">
      {toasts.map((toast) => (
        <div className={`toast toast-${toast.operation}`} role="status" key={toast.toastId}>
          <div>
            <strong>Product {operationLabels[toast.operation]}</strong>
            <p>{toast.name} ({toast.sku})</p>
          </div>
          <button
            type="button"
            aria-label={`Dismiss ${toast.name} notification`}
            onClick={() => onDismiss(toast.toastId)}
          >
            &times;
          </button>
        </div>
      ))}
    </div>
  );
}
