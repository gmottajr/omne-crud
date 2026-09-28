using System;
using System.Collections.Generic;
using System.Text;
using Omne_Crud_Demo.Core.Exceptions;

namespace Omne_Crud_Demo.Domain.Tests.Entities;

public sealed class ProductTests
{
    private const string SKU_CONST = "ABC-123";

    [Fact]
    public void Constructor_ShouldCreateProduct_WithValidValues()
    {
        var sku = GetSku();

        var product = new Product(
            " Keyboard ",
            Price.Create(99.90m),
            " Mechanical keyboard ",
            sku);

        Assert.Equal(sku, product.Sku);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal(99.90m, product.Price.Value);
        Assert.Equal("Mechanical keyboard", product.Description);
    }

    

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenNameEmpty()
    {
        var sku = GetSku();

        Assert.Throws<ArgumentException>(() =>
            new Product(
                " ",
                Price.Create(99.90m),
                "Mechanical keyboard",
                sku));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WithNegativePrice()
    {
        var sku = GetSku();

        Assert.Throws<InvalidPriceException>(() =>
            new Product(
                "Keyboard",
                Price.Create(-1m),
                "Mechanical keyboard",
                sku));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WithEmptyDescription()
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(
                "Keyboard",
                Price.Create(99.90m),
                " ",
                GetSku()));
    }

    [Fact]
    public void Update_ShouldChangeMutableProductProperties()
    {
        var sku = GetSku();

        var product = new Product(
            "Keyboard",
            Price.Create(99.90m),
            "Mechanical keyboard",
            sku);

        product.Update(
            "Mouse",
            Price.Create(49.90m),
            "Wireless mouse");

        Assert.Equal("Mouse", product.Name);
        Assert.Equal(49.90m, product.Price.Value);
        Assert.Equal("Wireless mouse", product.Description);
        Assert.Equal(sku, product.Sku);
    }

    [Fact]
    public void Update_ShouldRaiseProductUpdatedDomainEvent()
    {
        var product = new Product(
            "Keyboard",
            Price.Create(99.90m),
            "Mechanical keyboard",
            GetSku());
        product.ClearDomainEvents();

        product.Update(
            "Mouse",
            Price.Create(49.90m),
            "Wireless mouse");

        var updated = Assert.IsType<ProductUpdatedDomainEvent>(
            Assert.Single(product.DomainEvents));
        Assert.Equal("Mouse", updated.Name);
        Assert.Equal(49.90m, updated.Price);
        Assert.Equal(SKU_CONST, updated.Sku);
    }

    [Fact]
    public void MarkAsDeleted_ShouldRaiseProductDeletedDomainEvent()
    {
        var product = new Product(
            "Keyboard",
            Price.Create(99.90m),
            "Mechanical keyboard",
            GetSku());
        product.ClearDomainEvents();

        product.MarkAsDeleted();

        var deleted = Assert.IsType<ProductDeletedDomainEvent>(
            Assert.Single(product.DomainEvents));
        Assert.Equal("Keyboard", deleted.Name);
        Assert.Equal(SKU_CONST, deleted.Sku);
    }

    [Fact]
    public void Constructor_ShouldRaiseProductCreatedDomainEvent()
    {
        var sku = GetSku();

        var product = new Product(
            "Keyboard",
            Price.Create(99.90m),
            "Mechanical keyboard",
            sku);

        var domainEvent =
            Assert.Single(product.DomainEvents);

        var productCreated =
            Assert.IsType<ProductCreatedDomainEvent>(domainEvent);

        Assert.Equal(SKU_CONST, productCreated.Sku);
        Assert.Equal("Keyboard", productCreated.Name);
        Assert.Equal(99.90m, productCreated.Price);
    }

    private static Sku GetSku()
    {
        return Sku.Create("abc-123");
    }
}
