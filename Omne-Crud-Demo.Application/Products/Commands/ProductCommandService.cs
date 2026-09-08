using Omne_Crud_Demo.Application.Abstractions.Persistence;
using Omne_Crud_Demo.Core.Common.Services;
using Omne_Crud_Demo.Core.Common.Services.Consts;
using Omne_Crud_Demo.Domain;

namespace Omne_Crud_Demo.Application;

public sealed class ProductCommandService : IProductCommandService
{
    private readonly IProductRepository _repository;

    public ProductCommandService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationResponse<int>> CreateAsync(CreateProductCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Sku sku;

        try
        {
            sku = Sku.Create(command.Sku);
        }
        catch (ArgumentException ex)
        {
            return ApplicationResponse<int>.Failure(ProductErrorCodes.InvalidSku, ex.Message);
        }

        if (await _repository.ExistsBySkuAsync(sku, ct))
        {
            return ApplicationResponse<int>.Failure(ProductErrorCodes.SkuAlreadyExists, $"A product with SKU '{sku.Value}' already exists.");
        }

        Product product;

        try
        {
            product = new Product(command.Name, command.Price, command.Description, sku);
        }
        catch (ArgumentException ex)
        {
            return ApplicationResponse<int>.Failure(ProductErrorCodes.InvalidProduct, ex.Message);
        }

        await _repository.AddAsync(product, ct);

        await _repository.SaveChangesAsync(ct);

        return ApplicationResponse<int>.Ok(product.Id);
    }

    public async Task<ApplicationResponse> UpdateAsync(UpdateProductCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var product = await _repository.GetByIdAsync(command.Id, ct);

        if (product is null)
        {
            return ApplicationResponse.Failure(ProductErrorCodes.NotFound, $"Product with ID '{command.Id}' was not found.");
        }

        try
        {
            product.Update(command.Name, command.Price, command.Description);
        }
        catch (ArgumentException ex)
        {
            return ApplicationResponse.Failure(ProductErrorCodes.InvalidProduct, ex.Message);
        }

        await _repository.SaveChangesAsync(ct);

        return ApplicationResponse.Ok();
    }

    public async Task<ApplicationResponse> DeleteAsync(DeleteProductCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var product = await _repository.GetByIdAsync(command.Id, ct);

        if (product is null)
        {
            return ApplicationResponse.Failure(ProductErrorCodes.NotFound, $"Product with ID '{command.Id}' was not found.");
        }

        var gotDeleted =  await _repository.DeleteAsync(command.Id, ct);

        if (!gotDeleted)
        {
            return ApplicationResponse.Failure(ProductErrorCodes.NotFound, $"Product with ID '{command.Id}' was not found.");
        }

        return ApplicationResponse.Ok();
    }
}