using Omne_Crud_Demo.Abstractions;

namespace Omne_Crud_Demo.Domain;


public sealed class Product : AggregateRoot<int>
{
    private Product()
    {
    }

    public Product(
        string name,
        Price price,
        string description,
        Sku sku)
    {
        ArgumentNullException.ThrowIfNull(price);
        ArgumentNullException.ThrowIfNull(sku);

        SetName(name);
        SetPrice(price);
        SetDescription(description);

        Sku = sku;

        //In order to demostrate the use of domain events, we will raise a ProductCreatedDomainEvent when a new product is created.
        Raise(new ProductCreatedDomainEvent(
            Name,
            Price.Value,
            Description,
            Sku.Value));
    }

    public string Name { get; private set; } = string.Empty;

    public Price Price { get; private set; } = null!;

    public string Description { get; private set; } = string.Empty;

    public Sku Sku { get; private set; } = null!;

    public void Update(
        string name,
        Price price,
        string description)
    {
        ArgumentNullException.ThrowIfNull(price);

        SetName(name);
        SetPrice(price);
        SetDescription(description);
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = name.Trim();
        if (Name != normalizedName)
            Name = normalizedName;
    }

    private void SetPrice(Price price)
    {
        ArgumentNullException.ThrowIfNull(price);

        if (Price != price)
            Price = price;
    }

    private void SetDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        
        var normalizedDescription = description.Trim();
        if (Description != normalizedDescription)
            Description = normalizedDescription;
    }

}