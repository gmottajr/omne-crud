using FastEndpoints;
using Omne_Crud_Demo.Application;
using Omne_Crud_Demo.Core.Models.Requests;

namespace OmneCrudDemo.Server.Endpoints.Products;

public sealed class FilterProductsEndpoint : Endpoint<FilterProductsRequest>
{
    private readonly IProductQueryService _service;

    public FilterProductsEndpoint(IProductQueryService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("/products/filter");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        FilterProductsRequest request,
        CancellationToken ct)
    {
        var query = new FilterProductsQuery(
            request.Name,
            request.Sku,
            request.MinPrice,
            request.MaxPrice,
            request.CreatedFrom,
            request.CreatedTo,
            request.UpdatedFrom,
            request.UpdatedTo);

        var result = await _service.FilterAsync(query, ct);

        await Send.ResponseAsync(
            result,
            result.Success
                ? StatusCodes.Status200OK
                : StatusCodes.Status400BadRequest);
    }
}
