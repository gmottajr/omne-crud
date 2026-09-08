using FastEndpoints;
using Omne_Crud_Demo.Application;
using Omne_Crud_Demo.Core.Common.Services.Consts;

namespace OmneCrudDemo.Server.Endpoints.Products;

public sealed class GetProductBySkuEndpoint : EndpointWithoutRequest
{
    private readonly IProductQueryService _service;

    public GetProductBySkuEndpoint(IProductQueryService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("/products/sku/{sku}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var sku = Route<string>("sku");

        var result = await _service.GetBySkuAsync(
            new GetProductBySkuQuery(sku),
            ct);

        if (!result.Success)
        {
            var statusCode =
                result.ErrorCode == ProductErrorCodes.InvalidSku
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status404NotFound;

            await Send.ResponseAsync(result, statusCode);

            return;
        }

        await Send.OkAsync(result);
    }
}
