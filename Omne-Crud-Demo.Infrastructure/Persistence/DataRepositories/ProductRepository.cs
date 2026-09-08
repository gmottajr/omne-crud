using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Omne_Crud_Demo.Application.Abstractions.Persistence;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;

namespace Omne_Crud_Demo.Infrastructure.Persistence.DataRepositories;

public sealed class ProductRepository : DataRepository<Product, int>, IProductRepository
{
    public ProductRepository(AppDbContext context, ILogger<DataRepository<Product, int>> logger): base(context, logger)
    {
    }

    public Task<Product?> GetBySkuAsync(Sku sku, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sku);

        return QuerySingleAsync(product => product.Sku == sku, ct: ct);
    }

    public Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sku);

        return _dbSet.AnyAsync(product => product.Sku == sku, ct);
    }
}
