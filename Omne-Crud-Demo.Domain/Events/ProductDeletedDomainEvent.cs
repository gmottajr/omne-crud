using Omne_Crud_Demo.Abstractions;

namespace Omne_Crud_Demo.Domain;

public sealed class ProductDeletedDomainEvent(
    int productId,
    string name,
    string sku) : DomainEventBase
{
    public int ProductId { get; } = productId;

    public string Name { get; } = name;

    public string Sku { get; } = sku;
}
