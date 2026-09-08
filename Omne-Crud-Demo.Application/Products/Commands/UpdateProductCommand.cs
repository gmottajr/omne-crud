namespace Omne_Crud_Demo.Application;

public sealed record UpdateProductCommand(
    int Id,
    string Name,
    decimal Price,
    string Description);