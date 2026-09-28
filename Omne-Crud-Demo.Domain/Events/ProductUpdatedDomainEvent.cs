using Omne_Crud_Demo.Abstractions;

namespace Omne_Crud_Demo.Domain;

public sealed class ProductUpdatedDomainEvent(
    int productId,
    string name,
    decimal price,
    string description,
    string sku) : DomainEventBase
{
    public int ProductId { get; } = productId;

    public string Name { get; } = name;

    public decimal Price { get; } = price;

    public string Description { get; } = description;

    public string Sku { get; } = sku;
}
