using Omne_Crud_Demo.Application.Abstractions.Persistence;
using Omne_Crud_Demo.Application.Mappings;
using Omne_Crud_Demo.Core.Common.Services;
using Omne_Crud_Demo.Core.Common.Services.Consts;
using Omne_Crud_Demo.Core.Models;
using Omne_Crud_Demo.Domain;

namespace Omne_Crud_Demo.Application;

public sealed class ProductQueryService : IProductQueryService
{
    private readonly IProductRepository _repository;

    public ProductQueryService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationResponse<ProductDto>> GetByIdAsync(GetProductByIdQuery query, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(
            query.Id,
            ct);

        if (product is null)
        {
            return ApplicationResponse<ProductDto>.Failure(
                ProductErrorCodes.NotFound,
                $"Product with ID '{query.Id}' was not found.");
        }

        return ApplicationResponse<ProductDto>.Ok(
            ProductMapper.ToDto(product));
    }

    public async Task<ApplicationResponse<ProductDto>> GetBySkuAsync(
        GetProductBySkuQuery query,
        CancellationToken ct = default)
    {
        Sku sku;

        try
        {
            sku = Sku.Create(query.Sku);
        }
        catch (ArgumentException ex)
        {
            return ApplicationResponse<ProductDto>.Failure(
                ProductErrorCodes.InvalidSku,
                ex.Message);
        }

        var product = await _repository.GetBySkuAsync(
            sku,
            ct);

        if (product is null)
        {
            return ApplicationResponse<ProductDto>.Failure(
                ProductErrorCodes.NotFound,
                $"Product with SKU '{sku.Value}' was not found.");
        }

        return ApplicationResponse<ProductDto>.Ok(
            ProductMapper.ToDto(product));
    }

    public async Task<ApplicationResponse<IReadOnlyList<ProductDto>>> GetAllAsync(
        GetProductsQuery query,
        CancellationToken ct = default)
    {
        var products = await _repository.GetAllAsync(
            orderBy: products =>
                products.OrderBy(product => product.Name),
            ct: ct);

        var result = ProductMapper.ToDto(products);

        return ApplicationResponse<IReadOnlyList<ProductDto>>.Ok(
            result);
    }

    public async Task<ApplicationResponse<IReadOnlyList<ProductDto>>> FilterAsync(
        FilterProductsQuery query,
        CancellationToken ct = default)
    {
        var normalizedQuery = query with
        {
            Name = string.IsNullOrWhiteSpace(query.Name)
                ? null
                : query.Name.Trim().ToLowerInvariant(),
            Sku = string.IsNullOrWhiteSpace(query.Sku)
                ? null
                : query.Sku.Trim().ToUpperInvariant(),
            CreatedFrom = NormalizeDateTime(query.CreatedFrom),
            CreatedTo = NormalizeDateTime(query.CreatedTo),
            UpdatedFrom = NormalizeDateTime(query.UpdatedFrom),
            UpdatedTo = NormalizeDateTime(query.UpdatedTo)
        };

        var validationError = Validate(normalizedQuery);

        if (validationError is not null)
        {
            return ApplicationResponse<IReadOnlyList<ProductDto>>.Failure(
                ProductErrorCodes.InvalidProduct,
                validationError);
        }

        var products = await _repository.QueryAsync(
            product =>
                (normalizedQuery.Name == null || product.Name.ToLower().Contains(normalizedQuery.Name)) &&
                (!normalizedQuery.MinPrice.HasValue || product.Price.Value >= normalizedQuery.MinPrice.Value) &&
                (!normalizedQuery.MaxPrice.HasValue || product.Price.Value <= normalizedQuery.MaxPrice.Value) &&
                (!normalizedQuery.CreatedFrom.HasValue || product.CreatedAt >= normalizedQuery.CreatedFrom.Value) &&
                (!normalizedQuery.CreatedTo.HasValue || product.CreatedAt <= normalizedQuery.CreatedTo.Value) &&
                (!normalizedQuery.UpdatedFrom.HasValue || product.UpdatedAt >= normalizedQuery.UpdatedFrom.Value) &&
                (!normalizedQuery.UpdatedTo.HasValue || product.UpdatedAt <= normalizedQuery.UpdatedTo.Value),
            orderBy: products => products.OrderBy(product => product.Name),
            ct: ct);

        var filteredProducts = products
            .Where(product =>
                normalizedQuery.Sku is null ||
                product.Sku.Value.Contains(normalizedQuery.Sku, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return ApplicationResponse<IReadOnlyList<ProductDto>>.Ok(
            ProductMapper.ToDto(filteredProducts));
    }

    private static string? Validate(FilterProductsQuery query)
    {
        if (query.MinPrice < 0 || query.MaxPrice < 0)
        {
            return "Price filters cannot be less than zero.";
        }

        if (query.MinPrice > query.MaxPrice)
        {
            return "MinPrice cannot be greater than MaxPrice.";
        }

        if (query.CreatedFrom > query.CreatedTo)
        {
            return "CreatedFrom cannot be later than CreatedTo.";
        }

        if (query.UpdatedFrom > query.UpdatedTo)
        {
            return "UpdatedFrom cannot be later than UpdatedTo.";
        }

        return null;
    }

    private static DateTime? NormalizeDateTime(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }
}
