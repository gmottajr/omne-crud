using Microsoft.Extensions.Logging.Abstractions;
using Omne_Crud_Demo.Application.Tests.Integration.Fixtures;
using Omne_Crud_Demo.Core.Common.Services.Consts;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;
using Omne_Crud_Demo.Infrastructure.Persistence.DataRepositories;
using Microsoft.EntityFrameworkCore;

namespace Omne_Crud_Demo.Application.Tests.Integration;

[Collection(ApplicationIntegrationCollection.Name)]
public sealed class ProductCommandServiceIntegrationTests
    : IAsyncLifetime
{
    private readonly ApplicationDatabaseFixture _fixture;

    public ProductCommandServiceIntegrationTests(
        ApplicationDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        return _fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateAsync_Should_Persist_Product()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var command = new CreateProductCommand(
            Name: "Mechanical Keyboard",
            Price: 249.90m,
            Description: "RGB mechanical keyboard",
            Sku: "SKU-001");

        // Act
        var response = await service.CreateAsync(command);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data > 0);

        context.ChangeTracker.Clear();

        var product = await context
            .Set<Product>()
            .SingleAsync(x => x.Id == response.Data);

        Assert.Equal("Mechanical Keyboard", product.Name);
        Assert.Equal(249.90m, product.Price);
        Assert.Equal(
            "RGB mechanical keyboard",
            product.Description);
        Assert.Equal("SKU-001", product.Sku.Value);
    }

    [Fact]
    public async Task CreateAsync_Should_Fail_When_Sku_Already_Exists()
    {
        // Arrange
        await SeedProductAsync(
            "Existing Product",
            10.99m,
            "Existing product",
            "SKU-001");

        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var command = new CreateProductCommand(
            Name: "Another Product",
            Price: 20.99m,
            Description: "Another product",
            Sku: "SKU-001");

        // Act
        var response = await service.CreateAsync(command);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(
            ProductErrorCodes.SkuAlreadyExists,
            response.ErrorCode);

        context.ChangeTracker.Clear();

        var count = await context
            .Set<Product>()
            .CountAsync();

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Product_Changes()
    {
        // Arrange
        var product = await SeedProductAsync(
            "Old Name",
            10.99m,
            "Old description",
            "SKU-001");

        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var command = new UpdateProductCommand(
            Id: product.Id,
            Name: "Updated Name",
            Price: 25.50m,
            Description: "Updated description");

        // Act
        var response = await service.UpdateAsync(command);

        // Assert
        Assert.True(response.Success);

        context.ChangeTracker.Clear();

        var persistedProduct = await context
            .Set<Product>()
            .SingleAsync(x => x.Id == product.Id);

        Assert.Equal("Updated Name", persistedProduct.Name);
        Assert.Equal(25.50m, persistedProduct.Price);
        Assert.Equal(
            "Updated description",
            persistedProduct.Description);

        Assert.Equal(
            "SKU-001",
            persistedProduct.Sku.Value);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var command = new UpdateProductCommand(
            Id: int.MaxValue,
            Name: "Updated Product",
            Price: 25.50m,
            Description: "Updated description");

        // Act
        var response = await service.UpdateAsync(command);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(
            ProductErrorCodes.NotFound,
            response.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Product()
    {
        // Arrange
        var product = await SeedProductAsync(
            "Product",
            10.99m,
            "Product description",
            "SKU-001");

        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var command = new DeleteProductCommand(
            Id: product.Id);

        // Act
        var response = await service.DeleteAsync(command);

        // Assert
        Assert.True(response.Success);

        context.ChangeTracker.Clear();

        var exists = await context
            .Set<Product>()
            .AnyAsync(x => x.Id == product.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var command = new DeleteProductCommand(
            Id: int.MaxValue);

        // Act
        var response = await service.DeleteAsync(command);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(
            ProductErrorCodes.NotFound,
            response.ErrorCode);
    }

    private static ProductCommandService CreateService(
        AppDbContext context)
    {
        var repository = new ProductRepository(
            context,
            NullLogger<DataRepository<Product, int>>.Instance);

        return new ProductCommandService(repository);
    }

    private async Task<Product> SeedProductAsync(
        string name,
        decimal price,
        string description,
        string sku)
    {
        await using var context = _fixture.CreateDbContext();

        var product = new Product(
            name,
            price,
            description,
            Sku.Create(sku));

        context.Set<Product>().Add(product);

        await context.SaveChangesAsync();

        return product;
    }
}
