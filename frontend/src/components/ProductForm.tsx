import { useId, useState, type FormEvent } from 'react';
import type { CreateProductRequest, Product, UpdateProductRequest } from '../types/product';

interface BaseProductFormProps {
  disabled?: boolean;
}

interface CreateProductFormProps extends BaseProductFormProps {
  mode: 'create';
  product?: never;
  onSubmit: (request: CreateProductRequest) => void | Promise<void>;
  onCancel?: never;
}

interface EditProductFormProps extends BaseProductFormProps {
  mode: 'edit';
  product: Product;
  onSubmit: (request: UpdateProductRequest) => void | Promise<void>;
  onCancel: () => void;
}

type ProductFormProps = CreateProductFormProps | EditProductFormProps;

interface ProductFormValues {
  sku: string;
  name: string;
  price: string;
  description: string;
}

type ProductFormErrors = Partial<Record<keyof ProductFormValues, string>>;

function initialValues(product?: Product): ProductFormValues {
  return {
    sku: product?.sku ?? '',
    name: product?.name ?? '',
    price: product?.price.toString() ?? '',
    description: product?.description ?? ''
  };
}

function validate(values: ProductFormValues, mode: ProductFormProps['mode']): ProductFormErrors {
  const errors: ProductFormErrors = {};
  const sku = values.sku.trim();
  const name = values.name.trim();
  const description = values.description.trim();
  const price = Number(values.price);

  if (mode === 'create' && (sku.length < 4 || sku.length > 50)) {
    errors.sku = 'SKU must be between 4 and 50 characters.';
  }

  if (name.length < 3 || name.length > 200) {
    errors.name = 'Name must be between 3 and 200 characters.';
  }

  if (description.length < 10 || description.length > 1000) {
    errors.description = 'Description must be between 10 and 1,000 characters.';
  }

  if (
    values.price.trim() === '' ||
    !Number.isFinite(price) ||
    price < 0 ||
    !/^\d+(?:\.\d{1,2})?$/.test(values.price)
  ) {
    errors.price = 'Enter a non-negative price with up to two decimal places.';
  }

  return errors;
}

export function ProductForm(props: ProductFormProps) {
  const { mode, product, disabled = false } = props;
  const fieldId = useId();
  const [values, setValues] = useState<ProductFormValues>(() => initialValues(product));
  const [errors, setErrors] = useState<ProductFormErrors>({});

  const updateValue = (field: keyof ProductFormValues, value: string) => {
    setValues((current) => ({ ...current, [field]: value }));
    setErrors((current) => ({ ...current, [field]: undefined }));
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const validationErrors = validate(values, mode);
    setErrors(validationErrors);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    const commonRequest = {
      name: values.name.trim(),
      price: Number(values.price),
      description: values.description.trim()
    };

    if (props.mode === 'create') {
      await props.onSubmit({
        ...commonRequest,
        sku: values.sku.trim().toUpperCase()
      });
      return;
    }

    await props.onSubmit(commonRequest);
  };

  return (
    <form className="product-form" onSubmit={handleSubmit} noValidate>
      <div className="form-heading">
        <p className="eyebrow">{mode === 'create' ? 'Create' : 'Edit'}</p>
        <h2>{mode === 'create' ? 'New product' : 'Edit product'}</h2>
        <p>
          {mode === 'create'
            ? 'Enter the product details to add it to the catalog.'
            : 'Update the product details. The SKU cannot be changed.'}
        </p>
      </div>

      <label htmlFor={`${fieldId}-sku`}>
        SKU
        <input
          id={`${fieldId}-sku`}
          value={values.sku}
          onChange={(event) => updateValue('sku', event.target.value)}
          disabled={disabled}
          readOnly={mode === 'edit'}
          required={mode === 'create'}
          minLength={mode === 'create' ? 4 : undefined}
          maxLength={50}
          autoComplete="off"
          aria-invalid={Boolean(errors.sku)}
          aria-describedby={errors.sku ? `${fieldId}-sku-error` : undefined}
        />
        {errors.sku && <span id={`${fieldId}-sku-error`} className="field-error">{errors.sku}</span>}
      </label>

      <label htmlFor={`${fieldId}-name`}>
        Name
        <input
          id={`${fieldId}-name`}
          value={values.name}
          onChange={(event) => updateValue('name', event.target.value)}
          disabled={disabled}
          required
          minLength={3}
          maxLength={200}
          autoComplete="off"
          aria-invalid={Boolean(errors.name)}
          aria-describedby={errors.name ? `${fieldId}-name-error` : undefined}
        />
        {errors.name && <span id={`${fieldId}-name-error`} className="field-error">{errors.name}</span>}
      </label>

      <label htmlFor={`${fieldId}-price`}>
        Price
        <input
          id={`${fieldId}-price`}
          type="number"
          min="0"
          max="9999999999999999.99"
          step="0.01"
          inputMode="decimal"
          value={values.price}
          onChange={(event) => updateValue('price', event.target.value)}
          disabled={disabled}
          required
          aria-invalid={Boolean(errors.price)}
          aria-describedby={errors.price ? `${fieldId}-price-error` : undefined}
        />
        {errors.price && <span id={`${fieldId}-price-error`} className="field-error">{errors.price}</span>}
      </label>

      <label htmlFor={`${fieldId}-description`}>
        Description
        <textarea
          id={`${fieldId}-description`}
          value={values.description}
          onChange={(event) => updateValue('description', event.target.value)}
          disabled={disabled}
          required
          minLength={10}
          maxLength={1000}
          aria-invalid={Boolean(errors.description)}
          aria-describedby={errors.description ? `${fieldId}-description-error` : undefined}
        />
        <span className="field-hint">{values.description.length}/1,000 characters</span>
        {errors.description && (
          <span id={`${fieldId}-description-error`} className="field-error">{errors.description}</span>
        )}
      </label>

      <div className="form-actions">
        <button type="submit" disabled={disabled}>
          {disabled
            ? 'Saving...'
            : mode === 'create'
              ? 'Create product'
              : 'Save changes'}
        </button>
        {props.mode === 'edit' && (
          <button className="secondary-button" type="button" onClick={props.onCancel} disabled={disabled}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
