using Omne_Crud_Demo.Abstractions;
using Omne_Crud_Demo.Domain;

namespace Omne_Crud_Demo.Application.Abstractions.Persistence;

public interface IProductRepository : IDataRepositoryBase<Product, int>
{
    Task<Product?> GetBySkuAsync(
        Sku sku,
        CancellationToken ct = default);

    Task<bool> ExistsBySkuAsync(
        Sku sku,
        CancellationToken ct = default);
}
