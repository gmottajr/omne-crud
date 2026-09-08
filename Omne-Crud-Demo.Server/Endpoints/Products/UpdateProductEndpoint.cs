using FastEndpoints;
using Omne_Crud_Demo.Application;
using Omne_Crud_Demo.Core.Common.Services.Consts;
using Omne_Crud_Demo.Core.Models.Requests;

namespace OmneCrudDemo.Server.Endpoints.Products;

public sealed class UpdateProductEndpoint: Endpoint<UpdateProductRequest>
{
    private readonly IProductCommandService _service;

    public UpdateProductEndpoint(IProductCommandService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Put("/products/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        UpdateProductRequest request,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            request.Id,
            request.Name,
            request.Price,
            request.Description);

        var result = await _service.UpdateAsync(command, ct);

        if (!result.Success)
        {
            var statusCode =
                result.ErrorCode == ProductErrorCodes.NotFound
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest;

            await Send.ResponseAsync(result, statusCode);

            return;
        }

        await Send.ResponseAsync(
            result,
            StatusCodes.Status200OK);
    }
}