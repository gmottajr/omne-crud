namespace Omne_Crud_Demo.Application;

public sealed record CreateProductCommand(
    string Name,
    decimal Price,
    string Description,
    string Sku);