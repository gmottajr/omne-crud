import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError } from '../types/product';
import { httpClient } from './httpClient';

function response(body: unknown, status = 200, contentType = 'application/json'): Response {
  return {
    ok: status >= 200 && status < 300,
    status,
    headers: new Headers(contentType ? { 'content-type': contentType } : undefined),
    text: vi.fn(async () => body === undefined ? '' : JSON.stringify(body))
  } as Response;
}

describe('httpClient', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('desembrulha dados de ApplicationResponse', async () => {
    const fetchMock = vi.fn().mockResolvedValue(response({ success: true, data: [1, 2] }));
    vi.stubGlobal('fetch', fetchMock);

    await expect(httpClient.get<number[]>('/products')).resolves.toEqual([1, 2]);
    expect(fetchMock).toHaveBeenCalledWith('/products', expect.objectContaining({ method: 'GET' }));
  });

  it('aceita update bem-sucedido sem propriedade data', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(response({ success: true })));

    await expect(httpClient.put('/products/7', { name: 'Produto' })).resolves.toBeUndefined();
  });

  it('aceita delete com 204 sem tentar interpretar JSON', async () => {
    const bodylessResponse = response(undefined, 204, '');
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(bodylessResponse));

    await expect(httpClient.delete('/products/7')).resolves.toBeUndefined();
    expect(bodylessResponse.text).not.toHaveBeenCalled();
  });

  it('preserva errorMessage, errorCode e status de erro funcional', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(response({
      success: false,
      errorCode: 'PRODUCT_SKU_ALREADY_EXISTS',
      errorMessage: 'SKU já cadastrado.'
    }, 400)));

    const error = await httpClient.post('/products', {}).catch((caught: unknown) => caught);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({
      message: 'SKU já cadastrado.',
      errorCode: 'PRODUCT_SKU_ALREADY_EXISTS',
      status: 400,
      isNetworkError: false
    });
  });

  it('normaliza falha de rede', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('offline')));

    const error = await httpClient.get('/products').catch((caught: unknown) => caught);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({
      message: 'Não foi possível conectar à API.',
      isNetworkError: true
    });
  });
});
