using System;
using System.Collections.Generic;
using System.Text;

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
            99.90m,
            " Mechanical keyboard ",
            sku);

        Assert.Equal(sku, product.Sku);
        Assert.Equal("Keyboard", product.Name);
        Assert.Equal(99.90m, product.Price);
        Assert.Equal("Mechanical keyboard", product.Description);
    }

    

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenNameEmpty()
    {
        var sku = GetSku();

        Assert.Throws<ArgumentException>(() =>
            new Product(
                " ",
                99.90m,
                "Mechanical keyboard",
                sku));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WithNegativePrice()
    {
        var sku = GetSku();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Product(
                "Keyboard",
                -1m,
                "Mechanical keyboard",
                sku));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WithEmptyDescription()
    {
        Assert.Throws<ArgumentException>(() =>
            new Product(
                "Keyboard",
                99.90m,
                " ",
                GetSku()));
    }

    [Fact]
    public void Update_ShouldChangeMutableProductProperties()
    {
        var sku = GetSku();

        var product = new Product(
            "Keyboard",
            99.90m,
            "Mechanical keyboard",
            sku);

        product.Update(
            "Mouse",
            49.90m,
            "Wireless mouse");

        Assert.Equal("Mouse", product.Name);
        Assert.Equal(49.90m, product.Price);
        Assert.Equal("Wireless mouse", product.Description);
        Assert.Equal(sku, product.Sku);
    }

    [Fact]
    public void Constructor_ShouldRaiseProductCreatedDomainEvent()
    {
        var sku = GetSku();

        var product = new Product(
            "Keyboard",
            99.90m,
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