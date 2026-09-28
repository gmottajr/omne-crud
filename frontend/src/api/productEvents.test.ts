import { afterEach, describe, expect, it, vi } from 'vitest';
import { subscribeToProductEvents } from './productEvents';

class EventSourceStub {
  static instance: EventSourceStub | undefined;

  readonly listeners = new Map<string, (event: MessageEvent<string>) => void>();
  readonly close = vi.fn();
  readonly url: string;

  constructor(url: string) {
    this.url = url;
    EventSourceStub.instance = this;
  }

  addEventListener(type: string, listener: EventListenerOrEventListenerObject) {
    this.listeners.set(type, listener as (event: MessageEvent<string>) => void);
  }

  emit(type: string, data: string) {
    this.listeners.get(type)?.(new MessageEvent(type, { data }));
  }
}

describe('subscribeToProductEvents', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    EventSourceStub.instance = undefined;
  });

  it('receives named product events and closes the stream', () => {
    vi.stubGlobal('EventSource', EventSourceStub);
    const onNotification = vi.fn();

    const unsubscribe = subscribeToProductEvents(onNotification);
    EventSourceStub.instance?.emit('product', JSON.stringify({
      id: 'event-1',
      operation: 'updated',
      productId: 10,
      name: 'Keyboard',
      sku: 'SKU-001',
      occurredOn: '2026-09-28T10:00:00Z'
    }));

    expect(EventSourceStub.instance?.url).toBe('/events/products');
    expect(onNotification).toHaveBeenCalledWith(expect.objectContaining({
      operation: 'updated',
      sku: 'SKU-001'
    }));

    unsubscribe();
    expect(EventSourceStub.instance?.close).toHaveBeenCalledOnce();
  });
});
