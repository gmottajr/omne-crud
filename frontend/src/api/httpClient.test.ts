import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError } from '../types/product';
import { httpClient } from './httpClient';

function response(body: unknown, status = 200, contentType = 'application/json'): Response {
  const result = new Response(
    body === undefined ? null : JSON.stringify(body),
    {
      status,
      headers: contentType ? { 'content-type': contentType } : undefined
    }
  );

  vi.spyOn(result, 'text');

  return result;
}

describe('httpClient', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('unwraps data from ApplicationResponse', async () => {
    const fetchMock = vi.fn().mockResolvedValue(response({ success: true, data: [1, 2] }));
    vi.stubGlobal('fetch', fetchMock);

    await expect(httpClient.get<number[]>('/products')).resolves.toEqual([1, 2]);
    expect(fetchMock).toHaveBeenCalledWith('/products', expect.objectContaining({ method: 'GET' }));
  });

  it('accepts a successful update without a data property', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(response({ success: true })));

    await expect(httpClient.put('/products/7', { name: 'Product' })).resolves.toBeUndefined();
  });

  it('accepts a 204 delete without attempting to parse JSON', async () => {
    const bodylessResponse = response(undefined, 204, '');
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(bodylessResponse));

    await expect(httpClient.delete('/products/7')).resolves.toBeUndefined();
    expect(bodylessResponse.text).not.toHaveBeenCalled();
  });

  it('preserves errorMessage, errorCode, and functional error status', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(response({
      success: false,
      errorCode: 'PRODUCT_SKU_ALREADY_EXISTS',
      errorMessage: 'SKU already exists.'
    }, 400)));

    const error = await httpClient.post('/products', {}).catch((caught: unknown) => caught);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({
      message: 'SKU already exists.',
      errorCode: 'PRODUCT_SKU_ALREADY_EXISTS',
      status: 400,
      isNetworkError: false
    });
  });

  it('normalizes a network failure', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')));

    const error = await httpClient.get('/products').catch((caught: unknown) => caught);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({
      message: 'Unable to connect to the API.',
      isNetworkError: true
    });
  });
});
