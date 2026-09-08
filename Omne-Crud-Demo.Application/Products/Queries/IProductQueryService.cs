using Omne_Crud_Demo.Core.Common.Services;
using Omne_Crud_Demo.Core.Models;

namespace Omne_Crud_Demo.Application;

public interface IProductQueryService
{
    Task<ApplicationResponse<ProductDto>> GetByIdAsync(GetProductByIdQuery query, CancellationToken ct = default);

    Task<ApplicationResponse<ProductDto>> GetBySkuAsync(GetProductBySkuQuery query, CancellationToken ct = default);

    Task<ApplicationResponse<IReadOnlyList<ProductDto>>> GetAllAsync(GetProductsQuery query, CancellationToken ct = default);
}
