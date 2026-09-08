using FastEndpoints;
using Omne_Crud_Demo.Application;
using Omne_Crud_Demo.Core.Models.Requests;

namespace OmneCrudDemo.Server.Endpoints.Products;

public sealed class CreateProductEndpoint: Endpoint<CreateProductRequest>
{
    private readonly IProductCommandService _service;

    public CreateProductEndpoint(IProductCommandService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Post("/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        CreateProductRequest request,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Price,
            request.Description, 
            request.Sku);

        var result = await _service.CreateAsync(command, ct);

        if (!result.Success)
        {
            await Send.ResponseAsync(
                result,
                statusCode: StatusCodes.Status400BadRequest);

            return;
        }

        await Send.ResponseAsync(
            result,
            statusCode: StatusCodes.Status201Created);
    }
}

