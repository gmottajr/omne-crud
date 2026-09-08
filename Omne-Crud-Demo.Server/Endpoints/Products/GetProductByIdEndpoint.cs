using FastEndpoints;
using Omne_Crud_Demo.Application;

namespace OmneCrudDemo.Server.Endpoints.Products;

public sealed class GetProductByIdEndpoint : EndpointWithoutRequest
{
    private readonly IProductQueryService _service;

    public GetProductByIdEndpoint(IProductQueryService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("/products/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");

        var result = await _service.GetByIdAsync(new GetProductByIdQuery(id), ct);

        if (!result.Success)
        {
            await Send.ResponseAsync(
                result,
                StatusCodes.Status404NotFound);

            return;
        }

        await Send.OkAsync(result);
    }
}
