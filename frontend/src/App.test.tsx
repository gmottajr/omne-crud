import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import App from './App';
import { productsApi } from './api/productsApi';
import { ApiError, type Product } from './types/product';

vi.mock('./api/productsApi', () => ({
  productsApi: {
    getProducts: vi.fn(),
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
  name: 'Teclado',
  price: 249.9,
  description: 'Teclado mecânico para testes',
  createdAt: '2026-09-08T12:00:00Z',
  updatedAt: null
};

const mouse: Product = {
  id: 2,
  sku: 'SKU-002',
  name: 'Mouse',
  price: 99.9,
  description: 'Mouse ergonômico para testes',
  createdAt: '2026-09-08T13:00:00Z',
  updatedAt: null
};

async function waitForInitialLoad() {
  await screen.findByRole('heading', { name: 'Produtos cadastrados' });
  await waitFor(() => expect(productsApi.getProducts).toHaveBeenCalled());
}

async function fillCreateForm(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByRole('textbox', { name: 'SKU' }), 'sku-100');
  await user.type(screen.getByRole('textbox', { name: 'Nome' }), 'Monitor');
  await user.type(screen.getByRole('spinbutton', { name: 'Preço' }), '1599.90');
  await user.type(screen.getByRole('textbox', { name: /Descrição/ }), 'Monitor para estação de trabalho');
}

describe('App', () => {
  beforeEach(() => {
    vi.mocked(productsApi.getProducts).mockReset().mockResolvedValue([]);
    vi.mocked(productsApi.createProduct).mockReset().mockResolvedValue(10);
    vi.mocked(productsApi.updateProduct).mockReset().mockResolvedValue(undefined);
    vi.mocked(productsApi.deleteProduct).mockReset().mockResolvedValue(undefined);
  });

  it('exibe loading inicial e renderiza os produtos retornados', async () => {
    let resolveProducts!: (products: Product[]) => void;
    vi.mocked(productsApi.getProducts).mockReturnValue(
      new Promise<Product[]>((resolve) => {
        resolveProducts = resolve;
      })
    );

    render(<App />);
    expect(screen.getByText('Carregando produtos...')).toBeInTheDocument();

    resolveProducts([keyboard]);

    expect(await screen.findByText('Teclado')).toBeInTheDocument();
    expect(screen.getByText('SKU: SKU-001')).toBeInTheDocument();
  });

  it('exibe estado vazio após uma resposta bem-sucedida sem produtos', async () => {
    render(<App />);

    expect(await screen.findByRole('heading', { name: 'Nenhum produto cadastrado' })).toBeInTheDocument();
  });

  it('envia CreateProductRequest com SKU normalizado', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts)
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([keyboard]);
    render(<App />);
    await waitForInitialLoad();
    await fillCreateForm(user);

    await user.click(screen.getByRole('button', { name: 'Cadastrar produto' }));

    await waitFor(() => expect(productsApi.createProduct).toHaveBeenCalledWith({
      sku: 'SKU-100',
      name: 'Monitor',
      price: 1599.9,
      description: 'Monitor para estação de trabalho'
    }));
    expect(await screen.findByText('Produto cadastrado com sucesso.')).toBeInTheDocument();
  });

  it('envia UpdateProductRequest sem SKU ou ID no body', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts)
      .mockResolvedValueOnce([keyboard])
      .mockResolvedValueOnce([{ ...keyboard, name: 'Teclado atualizado' }]);
    render(<App />);

    await user.click(await screen.findByRole('button', { name: 'Editar' }));
    const skuInput = screen.getByRole('textbox', { name: 'SKU' });
    const nameInput = screen.getByRole('textbox', { name: 'Nome' });
    expect(skuInput).toHaveAttribute('readonly');
    await user.clear(nameInput);
    await user.type(nameInput, 'Teclado atualizado');
    await user.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    await waitFor(() => expect(productsApi.updateProduct).toHaveBeenCalledWith(1, {
      name: 'Teclado atualizado',
      price: 249.9,
      description: 'Teclado mecânico para testes'
    }));
    expect(vi.mocked(productsApi.updateProduct).mock.calls[0]![1]).not.toHaveProperty('sku');
    expect(vi.mocked(productsApi.updateProduct).mock.calls[0]![1]).not.toHaveProperty('id');
  });

  it('confirma e remove localmente um produto após DELETE', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts).mockResolvedValue([keyboard]);
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    render(<App />);

    await user.click(await screen.findByRole('button', { name: 'Excluir Teclado' }));

    expect(confirmSpy).toHaveBeenCalledWith(expect.stringContaining('Teclado'));
    await waitFor(() => expect(productsApi.deleteProduct).toHaveBeenCalledWith(1));
    expect(await screen.findByRole('heading', { name: 'Nenhum produto cadastrado' })).toBeInTheDocument();
  });

  it('apresenta erro funcional e preserva o formulário', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.createProduct).mockRejectedValue(new ApiError('SKU já cadastrado.', {
      status: 400,
      errorCode: 'PRODUCT_SKU_ALREADY_EXISTS'
    }));
    render(<App />);
    await waitForInitialLoad();
    await fillCreateForm(user);

    await user.click(screen.getByRole('button', { name: 'Cadastrar produto' }));

    expect(await screen.findByText('SKU já cadastrado.')).toBeInTheDocument();
    expect(screen.getByRole('textbox', { name: 'Nome' })).toHaveValue('Monitor');
  });

  it('preserva produtos existentes quando um refresh falha', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts)
      .mockResolvedValueOnce([keyboard])
      .mockRejectedValueOnce(new ApiError('Não foi possível conectar à API.', { isNetworkError: true }));
    render(<App />);
    expect(await screen.findByText('Teclado')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Atualizar lista' }));

    expect(await screen.findByText('Não foi possível conectar à API.')).toBeInTheDocument();
    expect(screen.getByText('Teclado')).toBeInTheDocument();
  });

  it('impede submissão duplicada durante a criação', async () => {
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

    const submit = screen.getByRole('button', { name: 'Cadastrar produto' });
    await user.click(submit);
    expect(screen.getByRole('button', { name: 'Salvando...' })).toBeDisabled();
    await user.click(screen.getByRole('button', { name: 'Salvando...' }));
    expect(productsApi.createProduct).toHaveBeenCalledTimes(1);

    resolveCreate(10);
    expect(await screen.findByText('Produto cadastrado com sucesso.')).toBeInTheDocument();
  });

  it('reinicializa o formulário ao cancelar ou trocar o produto em edição', async () => {
    const user = userEvent.setup();
    vi.mocked(productsApi.getProducts).mockResolvedValue([keyboard, mouse]);
    render(<App />);

    const keyboardCard = (await screen.findByText('Teclado')).closest('article');
    if (!keyboardCard) {
      throw new Error('Card do teclado não encontrado.');
    }

    await user.click(within(keyboardCard).getByRole('button', { name: 'Editar' }));
    const nameInput = screen.getByRole('textbox', { name: 'Nome' });
    await user.clear(nameInput);
    await user.type(nameInput, 'Valor temporário');
    await user.click(screen.getByRole('button', { name: 'Cancelar' }));

    expect(screen.getByRole('textbox', { name: 'Nome' })).toHaveValue('');

    const mouseCard = screen.getByText('Mouse').closest('article');
    if (!mouseCard) {
      throw new Error('Card do mouse não encontrado.');
    }

    await user.click(within(mouseCard).getByRole('button', { name: 'Editar' }));
    expect(screen.getByRole('textbox', { name: 'Nome' })).toHaveValue('Mouse');
    expect(screen.getByRole('textbox', { name: 'SKU' })).toHaveValue('SKU-002');
  });
});
