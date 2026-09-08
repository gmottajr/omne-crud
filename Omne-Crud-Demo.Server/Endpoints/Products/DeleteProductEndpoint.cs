using FastEndpoints;
using Omne_Crud_Demo.Application;

namespace OmneCrudDemo.Server.Endpoints.Products;

public sealed class DeleteProductEndpoint : EndpointWithoutRequest
{
    private readonly IProductCommandService _service;

    public DeleteProductEndpoint(IProductCommandService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Delete("/products/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");

        var result = await _service.DeleteAsync(
            new DeleteProductCommand(id),
            ct);

        if (!result.Success)
        {
            await Send.ResponseAsync(
                result,
                StatusCodes.Status404NotFound);

            return;
        }

        await Send.NoContentAsync();
    }
}
