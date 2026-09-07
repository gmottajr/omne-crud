using Omne_Crud_Demo.Abstractions;

namespace Omne_Crud_Demo.Domain;


public sealed class Product : AggregateRoot<int>
{
    private Product()
    {
    }

    public Product(
        string name,
        decimal price,
        string description)
    {
        SetName(name);
        SetPrice(price);
        SetDescription(description);

        //In order to demostrate the use of domain events, we will raise a ProductCreatedDomainEvent when a new product is created.
        Raise(new ProductCreatedDomainEvent(
            Name,
            Price,
            Description));
    }

    public string Name { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public void Update(
        string name,
        decimal price,
        string description)
    {
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

    private void SetPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "Price cannot be less than zero.");

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