using FastEndpoints;
using Omne_Crud_Demo.Application;

namespace OmneCrudDemo.Server.Endpoints.Products;

public sealed class GetProductsEndpoint : EndpointWithoutRequest
{
    private readonly IProductQueryService _service;

    public GetProductsEndpoint(IProductQueryService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _service.GetAllAsync(new GetProductsQuery(), ct);

        await Send.OkAsync(result);
    }
}
