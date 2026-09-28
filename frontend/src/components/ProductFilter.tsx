import { useState, type FormEvent } from 'react';
import type { ProductFilters } from '../types/product';

interface ProductFilterProps {
  disabled: boolean;
  onApply: (filters: ProductFilters) => void;
  onClear: () => void;
}

interface FilterFormValues {
  name: string;
  sku: string;
  minPrice: string;
  maxPrice: string;
  createdFrom: string;
  createdTo: string;
  updatedFrom: string;
  updatedTo: string;
}

const emptyValues: FilterFormValues = {
  name: '',
  sku: '',
  minPrice: '',
  maxPrice: '',
  createdFrom: '',
  createdTo: '',
  updatedFrom: '',
  updatedTo: ''
};

function startOfDay(value: string): string | undefined {
  return value ? `${value}T00:00:00.000Z` : undefined;
}

function endOfDay(value: string): string | undefined {
  return value ? `${value}T23:59:59.999Z` : undefined;
}

function optionalNumber(value: string): number | undefined {
  return value === '' ? undefined : Number(value);
}

export function ProductFilter({ disabled, onApply, onClear }: ProductFilterProps) {
  const [values, setValues] = useState<FilterFormValues>(emptyValues);

  const setValue = (field: keyof FilterFormValues, value: string) => {
    setValues((current) => ({ ...current, [field]: value }));
  };

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    onApply({
      name: values.name.trim() || undefined,
      sku: values.sku.trim() || undefined,
      minPrice: optionalNumber(values.minPrice),
      maxPrice: optionalNumber(values.maxPrice),
      createdFrom: startOfDay(values.createdFrom),
      createdTo: endOfDay(values.createdTo),
      updatedFrom: startOfDay(values.updatedFrom),
      updatedTo: endOfDay(values.updatedTo)
    });
  };

  const handleClear = () => {
    setValues(emptyValues);
    onClear();
  };

  return (
    <form className="product-filter" onSubmit={handleSubmit}>
      <div className="filter-heading">
        <div>
          <h3>Filter products</h3>
          <p>Combine any fields to narrow the catalog.</p>
        </div>
      </div>

      <div className="filter-grid">
        <label>
          Product name
          <input
            value={values.name}
            onChange={(event) => setValue('name', event.target.value)}
            placeholder="Partial name"
            disabled={disabled}
          />
        </label>
        <label>
          Product SKU
          <input
            value={values.sku}
            onChange={(event) => setValue('sku', event.target.value)}
            placeholder="Partial SKU"
            disabled={disabled}
          />
        </label>
        <label>
          Minimum price
          <input
            type="number"
            min="0"
            step="0.01"
            value={values.minPrice}
            onChange={(event) => setValue('minPrice', event.target.value)}
            disabled={disabled}
          />
        </label>
        <label>
          Maximum price
          <input
            type="number"
            min="0"
            step="0.01"
            value={values.maxPrice}
            onChange={(event) => setValue('maxPrice', event.target.value)}
            disabled={disabled}
          />
        </label>
        <label>
          Created from
          <input
            type="date"
            value={values.createdFrom}
            onChange={(event) => setValue('createdFrom', event.target.value)}
            disabled={disabled}
          />
        </label>
        <label>
          Created to
          <input
            type="date"
            value={values.createdTo}
            onChange={(event) => setValue('createdTo', event.target.value)}
            disabled={disabled}
          />
        </label>
        <label>
          Updated from
          <input
            type="date"
            value={values.updatedFrom}
            onChange={(event) => setValue('updatedFrom', event.target.value)}
            disabled={disabled}
          />
        </label>
        <label>
          Updated to
          <input
            type="date"
            value={values.updatedTo}
            onChange={(event) => setValue('updatedTo', event.target.value)}
            disabled={disabled}
          />
        </label>
      </div>

      <div className="filter-actions">
        <button type="submit" disabled={disabled}>
          {disabled ? 'Filtering...' : 'Apply filters'}
        </button>
        <button type="button" className="secondary-button" onClick={handleClear} disabled={disabled}>
          Clear filters
        </button>
      </div>
    </form>
  );
}
