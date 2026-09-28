export type ProductOperation = 'created' | 'updated' | 'deleted';

export interface ProductEventNotification {
  id: string;
  operation: ProductOperation;
  productId: number | null;
  name: string;
  sku: string;
  occurredOn: string;
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export function subscribeToProductEvents(
  onNotification: (notification: ProductEventNotification) => void
): () => void {
  if (typeof EventSource === 'undefined') {
    return () => undefined;
  }

  const eventSource = new EventSource(`${API_BASE_URL}/events/products`);

  eventSource.addEventListener('product', (event) => {
    try {
      onNotification(JSON.parse(event.data) as ProductEventNotification);
    } catch {
      // Ignore malformed events and keep the stream alive for later notifications.
    }
  });

  return () => eventSource.close();
}
