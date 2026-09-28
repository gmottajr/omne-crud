namespace Omne_Crud_Demo.Core.Models.Requests;

public sealed class FilterProductsRequest
{
    public string? Name { get; init; }

    public string? Sku { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public DateTime? CreatedFrom { get; init; }

    public DateTime? CreatedTo { get; init; }

    public DateTime? UpdatedFrom { get; init; }

    public DateTime? UpdatedTo { get; init; }
}
