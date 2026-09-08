using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Omne_Crud_Demo.Application.Tests.Integration.Fixtures;
using Omne_Crud_Demo.Core.Common.Services.Consts;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;
using Omne_Crud_Demo.Infrastructure.Persistence.DataRepositories;

namespace Omne_Crud_Demo.Application.Tests.Integration;

[Collection(ApplicationIntegrationCollection.Name)]
public sealed class ProductQueryServiceIntegrationTests: IAsyncLifetime
{
    private readonly ApplicationDatabaseFixture _fixture;

    public ProductQueryServiceIntegrationTests(ApplicationDatabaseFixture fixture)
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
    public async Task GetByIdAsync_Should_Return_Persisted_Product()
    {
        // Arrange
        var product = await SeedProductAsync(
            "Mechanical Keyboard",
            249.90m,
            "RGB mechanical keyboard",
            "SKU-001");

        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var query = new GetProductByIdQuery(
            Id: product.Id);

        // Act
        var response = await service.GetByIdAsync(query);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);

        Assert.Equal(product.Id, response.Data.Id);
        Assert.Equal("Mechanical Keyboard", response.Data.Name);
        Assert.Equal(249.90m, response.Data.Price);
        Assert.Equal(
            "RGB mechanical keyboard",
            response.Data.Description);
        Assert.Equal("SKU-001", response.Data.Sku);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var query = new GetProductByIdQuery(
            Id: int.MaxValue);

        // Act
        var response = await service.GetByIdAsync(query);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(
            ProductErrorCodes.NotFound,
            response.ErrorCode);
    }

    [Fact]
    public async Task GetBySkuAsync_Should_Return_Persisted_Product()
    {
        // Arrange
        var product = await SeedProductAsync(
            "Gaming Mouse",
            149.90m,
            "Wireless gaming mouse",
            "SKU-002");

        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var query = new GetProductBySkuQuery(
            Sku: "SKU-002");

        // Act
        var response = await service.GetBySkuAsync(query);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);

        Assert.Equal(product.Id, response.Data.Id);
        Assert.Equal("Gaming Mouse", response.Data.Name);
        Assert.Equal("SKU-002", response.Data.Sku);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Products_Ordered_By_Name()
    {
        // Arrange
        await SeedProductAsync(
            "Mouse",
            100m,
            "Mouse",
            "SKU-001");

        await SeedProductAsync(
            "Keyboard",
            200m,
            "Keyboard",
            "SKU-002");

        await SeedProductAsync(
            "Display",
            500m,
            "Display",
            "SKU-003");

        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var query = new GetProductsQuery();

        // Act
        var response = await service.GetAllAsync(query);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);

        Assert.Collection(
            response.Data,
            product => Assert.Equal("Display", product.Name),
            product => Assert.Equal("Keyboard", product.Name),
            product => Assert.Equal("Mouse", product.Name));
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Empty_List_When_No_Products_Exist()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        var service = CreateService(context);

        var query = new GetProductsQuery();

        // Act
        var response = await service.GetAllAsync(query);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
    }

    private static ProductQueryService CreateService(
        AppDbContext context)
    {
        var repository = new ProductRepository(
            context,
            NullLogger<DataRepository<Product, int>>.Instance);

        return new ProductQueryService(repository);
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
