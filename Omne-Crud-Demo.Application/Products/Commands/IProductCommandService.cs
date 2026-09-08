using Omne_Crud_Demo.Core.Common.Services;

namespace Omne_Crud_Demo.Application;


public interface IProductCommandService
{
    Task<ApplicationResponse<int>> CreateAsync(CreateProductCommand command, CancellationToken ct = default);

    Task<ApplicationResponse> UpdateAsync(UpdateProductCommand command, CancellationToken ct = default);

    Task<ApplicationResponse> DeleteAsync(DeleteProductCommand command, CancellationToken ct = default);
}
