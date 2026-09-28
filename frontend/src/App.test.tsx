import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import App from './App';
import { productsApi } from './api/productsApi';
import { ApiError, type Product } from './types/product';

vi.mock('./api/productsApi', () => ({
  productsApi: {
    getProducts: vi.fn(),
    filterProducts: vi.fn(),
    getProductById: vi.fn(),
    getProductBySku: vi.fn(),
    createProduct: vi.fn(),
    updateProduct: vi.fn(),
    deleteProduct: vi.fn()
  }
}));

const keyboard: Product = {
  id: 1,
  sku: 'SKU-001',
  name: 'Keyboard',
  price: 249.9,
  description: 'Mechanical keyboard for testing',
  createdAt: '2026-09-08T12:00:00Z',
  updatedAt: null
};

const mouse: Product = {
  id: 2,
  sku: 'SKU-002',
  name: 'Mouse',
  price: 99.9,
  description: 'Ergonomic mouse for testing',
  createdAt: '2026-09-08T13:00:00Z',
  updatedAt: null
};

async function waitForInitialLoad() {
  await screen.findByRole('heading', { name: 'Registered products' });
  await waitFor(() => expect(productsApi.getProducts).toHaveBeenCalled());
}

async function fillCreateForm(user: ReturnType<typeof userEvent.setup>) {
  await user.click(screen.getByRole('button', { name: 'Add new product' }));
  await user.type(screen.getByRole('textbox', { name: 'SKU' }), 'sku-100');
  await user.type(screen.getByRole('textbox', { name: 'Name' }), 'Monitor');
  await user.type(screen.getByRole('spinbutton', { name: 'Price' }), '1599.90');
  await user.type(screen.getByRole('textbox', { name: /Description/ }), 'Monitor for a workstation');
}

describe('App', () => {
  beforeEach(() => {
    vi.mocked(productsApi.getProducts).mockReset().mockResolvedValue([]);
    vi.mocked(productsApi.filterProducts).mockReset().mockResolvedValue([]);
    vi.mocked(productsApi.createProduct).mockReset().mockResolvedValue(10);
    vi.mocked(productsApi.updateProduct).mockReset().mockResolvedValue(undefined);
    vi.mocked(productsApi.deleteProduct).mockReset().mockResolvedValue(undefined);
  });

  it('shows the initial loading state and renders returned products', async () => {
    let resolveProducts!: (products: Product[]) => void;
    vi.mocked(productsApi.getProducts).mockReturnValue(
      new Promise<Product[]>((resolve) => {
        resolveProducts = resolve;
      })
    );

    render(<App />);
    expect(screen.getByText('Loading products...')).toBeInTheDocument();

    resolveProducts([keyboard]);

    expect(await screen.findByText('Keyboard')).toBeInTheDocument();
    expect(screen.getByText('SKU: SKU-001')).toBeInTheDocument();
    expect(screen.queryByRole('heading', { name: 'New product' })).not.toBeInTheDocument();
  });

  it('shows the empty state after a successful response with no products', async () => {
    render(<App />);

    expect(await screen.findByRole('heading', { name: 'No products registered' })).toBeInTheDocument();
  });

  it('sends CreateProductRequest with a normalized SKU', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts)
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([keyboard]);
    render(<App />);
    await waitForInitialLoad();
    await fillCreateForm(user);

    await user.click(screen.getByRole('button', { name: 'Create product' }));

    await waitFor(() => expect(productsApi.createProduct).toHaveBeenCalledWith({
      sku: 'SKU-100',
      name: 'Monitor',
      price: 1599.9,
      description: 'Monitor for a workstation'
    }));
    expect(await screen.findByText('Product created successfully.')).toBeInTheDocument();
  });

  it('sends UpdateProductRequest without SKU or ID in the body', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts)
      .mockResolvedValueOnce([keyboard])
      .mockResolvedValueOnce([{ ...keyboard, name: 'Updated keyboard' }]);
    render(<App />);

    await user.click(await screen.findByRole('button', { name: 'Edit' }));
    const skuInput = screen.getByRole('textbox', { name: 'SKU' });
    const nameInput = screen.getByRole('textbox', { name: 'Name' });
    expect(skuInput).toHaveAttribute('readonly');
    await user.clear(nameInput);
    await user.type(nameInput, 'Updated keyboard');
    await user.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => expect(productsApi.updateProduct).toHaveBeenCalledWith(1, {
      name: 'Updated keyboard',
      price: 249.9,
      description: 'Mechanical keyboard for testing'
    }));
    expect(vi.mocked(productsApi.updateProduct).mock.calls[0]![1]).not.toHaveProperty('sku');
    expect(vi.mocked(productsApi.updateProduct).mock.calls[0]![1]).not.toHaveProperty('id');
  });

  it('confirms and removes a product locally after DELETE', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts).mockResolvedValue([keyboard]);
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    render(<App />);

    await user.click(await screen.findByRole('button', { name: 'Delete Keyboard' }));

    expect(confirmSpy).toHaveBeenCalledWith(expect.stringContaining('Keyboard'));
    await waitFor(() => expect(productsApi.deleteProduct).toHaveBeenCalledWith(1));
    expect(await screen.findByRole('heading', { name: 'No products registered' })).toBeInTheDocument();
  });

  it('shows a functional error and preserves the form', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.createProduct).mockRejectedValue(new ApiError('SKU already exists.', {
      status: 400,
      errorCode: 'PRODUCT_SKU_ALREADY_EXISTS'
    }));
    render(<App />);
    await waitForInitialLoad();
    await fillCreateForm(user);

    await user.click(screen.getByRole('button', { name: 'Create product' }));

    expect(await screen.findByText('SKU already exists.')).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: 'Name' })).toHaveValue('Monitor');
  });

  it('preserves existing products when a refresh fails', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts)
      .mockResolvedValueOnce([keyboard])
      .mockRejectedValueOnce(new ApiError('Unable to connect to the API.', { isNetworkError: true }));
    render(<App />);
    expect(await screen.findByText('Keyboard')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Refresh list' }));

    expect(await screen.findByText('Unable to connect to the API.')).toBeInTheDocument();
    expect(screen.getByText('Keyboard')).toBeInTheDocument();
  });

  it('prevents duplicate submission while creating', async () => {
    const user = userEvent.setup();
    let resolveCreate!: (id: number) => void;
    vi.mocked(productsApi.createProduct).mockReturnValue(
      new Promise<number>((resolve) => {
        resolveCreate = resolve;
      })
    );
    render(<App />);
    await waitForInitialLoad();
    await fillCreateForm(user);

    const submit = screen.getByRole('button', { name: 'Create product' });
    await user.click(submit);
    expect(screen.getByRole('button', { name: 'Saving...' })).toBeDisabled();
    await user.click(screen.getByRole('button', { name: 'Saving...' }));
    expect(productsApi.createProduct).toHaveBeenCalledTimes(1);

    resolveCreate(10);
    expect(await screen.findByText('Product created successfully.')).toBeInTheDocument();
  });

  it('resets the form when editing is canceled or another product is selected', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts).mockResolvedValue([keyboard, mouse]);
    render(<App />);

    const keyboardCard = (await screen.findByText('Keyboard')).closest('article');
    if (!keyboardCard) {
      throw new Error('Keyboard card was not found.');
    }

    await user.click(within(keyboardCard).getByRole('button', { name: 'Edit' }));
    const nameInput = screen.getByRole('textbox', { name: 'Name' });
    await user.clear(nameInput);
    await user.type(nameInput, 'Temporary value');
    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

    const mouseCard = screen.getByText('Mouse').closest('article');
    if (!mouseCard) {
      throw new Error('Mouse card was not found.');
    }

    await user.click(within(mouseCard).getByRole('button', { name: 'Edit' }));
    expect(screen.getByRole('dialog', { name: 'Edit Mouse' })).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: 'Name' })).toHaveValue('Mouse');
    expect(screen.getByRole('textbox', { name: 'SKU' })).toHaveValue('SKU-002');
  });

  it('sorts the loaded products in both directions without another API request', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts).mockResolvedValue([keyboard, mouse]);
    render(<App />);

    await screen.findByText(keyboard.name);
    const productList = screen.getByLabelText('Product list');
    const productNames = () =>
      within(productList).getAllByRole('heading', { level: 2 }).map((heading) => heading.textContent);

    const sortProducts = screen.getByRole('combobox', { name: 'Sort products' });

    expect(productNames()).toEqual([keyboard.name, mouse.name]);

    await user.selectOptions(sortProducts, 'name-desc');
    expect(productNames()).toEqual([mouse.name, keyboard.name]);

    await user.selectOptions(sortProducts, 'sku-asc');
    expect(productNames()).toEqual([keyboard.name, mouse.name]);

    await user.selectOptions(sortProducts, 'sku-desc');
    expect(productNames()).toEqual([mouse.name, keyboard.name]);

    await user.selectOptions(sortProducts, 'price-asc');
    expect(productNames()).toEqual([mouse.name, keyboard.name]);

    await user.selectOptions(sortProducts, 'price-desc');
    expect(productNames()).toEqual([keyboard.name, mouse.name]);

    await user.selectOptions(sortProducts, 'newest');
    expect(productNames()).toEqual([mouse.name, keyboard.name]);

    await user.selectOptions(sortProducts, 'oldest');
    expect(productNames()).toEqual([keyboard.name, mouse.name]);
    expect(productsApi.getProducts).toHaveBeenCalledTimes(1);
  });

  it('filters products through the API and clears the active filters', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts).mockResolvedValue([keyboard, mouse]);
    vi.mocked(productsApi.filterProducts).mockResolvedValue([keyboard]);
    render(<App />);

    await screen.findByText(keyboard.name);

    await user.type(screen.getByRole('textbox', { name: 'Product name' }), 'key');
    await user.type(screen.getByRole('textbox', { name: 'Product SKU' }), '001');
    await user.type(screen.getByRole('spinbutton', { name: 'Minimum price' }), '200');
    await user.type(screen.getByRole('spinbutton', { name: 'Maximum price' }), '300');
    fireEvent.change(screen.getByLabelText('Created from'), { target: { value: '2026-09-01' } });
    fireEvent.change(screen.getByLabelText('Created to'), { target: { value: '2026-09-30' } });
    fireEvent.change(screen.getByLabelText('Updated from'), { target: { value: '2026-09-02' } });
    fireEvent.change(screen.getByLabelText('Updated to'), { target: { value: '2026-09-29' } });

    await user.click(screen.getByRole('button', { name: 'Apply filters' }));

    await waitFor(() => expect(productsApi.filterProducts).toHaveBeenCalledWith({
      name: 'key',
      sku: '001',
      minPrice: 200,
      maxPrice: 300,
      createdFrom: '2026-09-01T00:00:00.000Z',
      createdTo: '2026-09-30T23:59:59.999Z',
      updatedFrom: '2026-09-02T00:00:00.000Z',
      updatedTo: '2026-09-29T23:59:59.999Z'
    }));
    expect(screen.getByText(keyboard.name)).toBeInTheDocument();
    expect(screen.queryByText(mouse.name)).not.toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Clear filters' }));

    await waitFor(() => expect(productsApi.getProducts).toHaveBeenCalledTimes(2));
    expect(screen.getByRole('textbox', { name: 'Product name' })).toHaveValue('');
    expect(await screen.findByText(mouse.name)).toBeInTheDocument();
  });
});
