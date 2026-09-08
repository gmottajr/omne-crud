import { ApiError, type ApplicationResponse } from '../types/product';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE';

async function request<TResponse, TRequest = undefined>(
  method: HttpMethod,
  url: string,
  body?: TRequest,
  expectsData = false
): Promise<TResponse> {
  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${url}`, {
      method,
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body)
    });
  } catch (cause) {
    throw new ApiError('Não foi possível conectar à API.', {
      isNetworkError: true,
      cause
    });
  }

  const responseBody = await readResponseBody(response);

  if (!response.ok) {
    throw createApiError(response.status, responseBody);
  }

  if (response.status === 204 || responseBody === undefined) {
    if (expectsData) {
      throw new ApiError('A API não retornou os dados esperados.', { status: response.status });
    }

    return undefined as TResponse;
  }

  if (!isApplicationResponse(responseBody)) {
    throw new ApiError('A API retornou uma resposta inválida.', { status: response.status });
  }

  if (!responseBody.success) {
    throw createApiError(response.status, responseBody);
  }

  if (!expectsData) {
    return undefined as TResponse;
  }

  if (!('data' in responseBody) || responseBody.data === undefined || responseBody.data === null) {
    throw new ApiError('A API não retornou os dados esperados.', { status: response.status });
  }

  return responseBody.data as TResponse;
}

async function readResponseBody(response: Response): Promise<unknown> {
  if (response.status === 204) {
    return undefined;
  }

  const text = await response.text();
  if (!text.trim()) {
    return undefined;
  }

  const contentType = response.headers.get('content-type');
  if (!contentType?.includes('json')) {
    return text;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch (cause) {
    throw new ApiError('A API retornou uma resposta inválida.', {
      status: response.status,
      cause
    });
  }
}

function isApplicationResponse(value: unknown): value is ApplicationResponse {
  return (
    typeof value === 'object' &&
    value !== null &&
    'success' in value &&
    typeof value.success === 'boolean'
  );
}

function createApiError(status: number, body: unknown): ApiError {
  if (isApplicationResponse(body)) {
    return new ApiError(body.errorMessage ?? 'A API não conseguiu concluir a operação.', {
      status,
      errorCode: body.errorCode
    });
  }

  if (isProblemDetails(body)) {
    return new ApiError(
      body.detail ?? body.title ?? `A API retornou um erro (HTTP ${status}).`,
      { status }
    );
  }

  return new ApiError(`A API retornou um erro (HTTP ${status}).`, { status });
}

function isProblemDetails(value: unknown): value is { title?: string; detail?: string } {
  if (typeof value !== 'object' || value === null) {
    return false;
  }

  const candidate = value as Record<string, unknown>;
  return (
    (candidate.title === undefined || typeof candidate.title === 'string') &&
    (candidate.detail === undefined || typeof candidate.detail === 'string') &&
    (typeof candidate.title === 'string' || typeof candidate.detail === 'string')
  );
}

export const httpClient = {
  get: <TResponse>(url: string) => request<TResponse>('GET', url, undefined, true),
  post: <TRequest, TResponse>(url: string, body: TRequest) =>
    request<TResponse, TRequest>('POST', url, body, true),
  put: <TRequest>(url: string, body: TRequest) =>
    request<void, TRequest>('PUT', url, body),
  delete: (url: string) => request<void>('DELETE', url)
};
