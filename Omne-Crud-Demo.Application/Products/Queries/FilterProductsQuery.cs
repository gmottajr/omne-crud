namespace Omne_Crud_Demo.Application;

public sealed record FilterProductsQuery(
    string? Name = null,
    string? Sku = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? UpdatedFrom = null,
    DateTime? UpdatedTo = null);
