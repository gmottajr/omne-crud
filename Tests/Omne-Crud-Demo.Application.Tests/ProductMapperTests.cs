using Omne_Crud_Demo.Application.Mappings;
using Omne_Crud_Demo.Domain;

namespace Omne_Crud_Demo.Application.Tests;

public sealed class ProductMapperTests
{
    [Fact]
    public void ToDto_Should_Map_All_Product_Properties()
    {
        // Arrange
        var product = CreateProduct(
            sku: "SKU-001",
            name: "Mechanical Keyboard",
            price: 249.90m,
            description: "RGB mechanical keyboard");

        // Act
        var result = ProductMapper.ToDto(product);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(product.Id, result.Id);
        Assert.Equal(product.Sku.Value, result.Sku);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Price, result.Price);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.CreatedAt, result.CreatedAt);
        Assert.Equal(product.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public void ToDto_Should_Map_Sku_Value_To_String()
    {
        // Arrange
        const string sku = "SKU-002";

        var product = CreateProduct(
            sku,
            "Gaming Mouse",
            149.90m,
            "Wireless gaming mouse");

        // Act
        var result = ProductMapper.ToDto(product);

        // Assert
        Assert.Equal(sku, result.Sku);
    }

    [Fact]
    public void ToDto_Should_Map_Product_Collection()
    {
        // Arrange
        IReadOnlyList<Product> products =
        [
            CreateProduct(
                "SKU-001",
                "Mechanical Keyboard",
                249.90m,
                "RGB mechanical keyboard"),

            CreateProduct(
                "SKU-002",
                "Gaming Mouse",
                149.90m,
                "Wireless gaming mouse")
        ];

        // Act
        var result = ProductMapper.ToDto(products);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("SKU-001", first.Sku);
                Assert.Equal("Mechanical Keyboard", first.Name);
                Assert.Equal(249.90m, first.Price);
                Assert.Equal("RGB mechanical keyboard", first.Description);
            },
            second =>
            {
                Assert.Equal("SKU-002", second.Sku);
                Assert.Equal("Gaming Mouse", second.Name);
                Assert.Equal(149.90m, second.Price);
                Assert.Equal("Wireless gaming mouse", second.Description);
            });
    }

    [Fact]
    public void ToDto_Should_Return_Empty_Collection_When_Source_Is_Empty()
    {
        // Arrange
        IReadOnlyList<Product> products = [];

        // Act
        var result = ProductMapper.ToDto(products);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private static Product CreateProduct(
        string sku,
        string name,
        decimal price,
        string description)
    {
        return new Product(
            name,
            price,
            description,
            Sku.Create(sku));
    }
}
