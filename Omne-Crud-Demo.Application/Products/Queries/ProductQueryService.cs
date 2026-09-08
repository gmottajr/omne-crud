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

    public async Task<ApplicationResponse<ProductDto>> GetByIdAsync(
        GetProductByIdQuery query,
        CancellationToken ct = default)
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
}
