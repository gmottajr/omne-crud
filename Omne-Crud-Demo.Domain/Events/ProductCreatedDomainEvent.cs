using Omne_Crud_Demo.Abstractions;

namespace Omne_Crud_Demo.Domain;

public sealed class ProductCreatedDomainEvent : DomainEventBase
{
    public ProductCreatedDomainEvent(
        string name,
        decimal price,
        string description)
    {
        Name = name;
        Price = price;
        Description = description;
    }

    public string Name { get; }

    public decimal Price { get; }

    public string Description { get; }
}
