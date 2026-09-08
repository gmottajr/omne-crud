using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;
using Omne_Crud_Demo.Infrastructure.Persistence.DataRepositories;
using Omne_Crud_Demo.Infrastructure.Tests.Integration;

namespace Omne_Crud_Demo.Infrastructure.Tests.Persistence;

[Collection(InfrastructureIntegrationCollection.Name)]
public sealed class ProductRepositoryTests
{
    private const string SKU_XXXV = "XXXV-555123";
    private const string SKU_WWWYYY = "WWW-YYY-4444";

    private readonly InfrastructureDatabaseFixture _fixture;

    public ProductRepositoryTests(InfrastructureDatabaseFixture fixture)
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

    private AppDbContext CreateContext()
    {
        return _fixture.CreateDbContext();
    }

    private static ProductRepository CreateRepository(AppDbContext context)
    {
        return new ProductRepository(context, NullLogger<DataRepository<Product, int>>.Instance);
    }

    private static Product CreateProduct(string sku, string name = "Keyboard",
        decimal price = 157.90m, string description = "Mechanical keyboard")
    {
        return new Product(
            name,
            price,
            description,
            Sku.Create(sku));
    }

    [Fact]
    public async Task GetBySkuAsync_ShouldReturnProduct_WhenSkuExists()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var product = CreateProduct(SKU_XXXV);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var result = await repository.GetBySkuAsync(
            Sku.Create(SKU_XXXV));

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(SKU_XXXV, result.Sku.Value);
        Assert.Equal("Keyboard", result.Name);
    }

    [Fact]
    public async Task GetBySkuAsync_ShouldReturnNull_WhenSkuDoesNotExist()
    {
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var product = CreateProduct(SKU_XXXV);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var result = await repository.GetBySkuAsync(
            Sku.Create("XYZ-999"));

        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsBySkuAsync_ShouldReturnTrue_WhenSkuExists()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var product = CreateProduct(SKU_XXXV);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var exists = await repository.ExistsBySkuAsync(
            Sku.Create(SKU_XXXV));

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsBySkuAsync_ShouldReturnFalse_WhenSkuDoesNotExist()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var product = CreateProduct(SKU_XXXV);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var exists = await repository.ExistsBySkuAsync(
            Sku.Create("XYZ-999"));

        Assert.False(exists);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();

        await context.Products.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task GetBySkuAsync_ShouldThrowArgumentNullException_WhenSkuIsNull()
    {
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repository.GetBySkuAsync(null!));
    }

    [Fact]
    public async Task ExistsBySkuAsync_ShouldThrowArgumentNullException_WhenSkuIsNull()
    {
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repository.ExistsBySkuAsync(null!));
    }

    [Fact]
    public async Task GetBySkuAsync_ShouldReturnCorrectProduct_WhenMultipleProductsExist()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await repository.AddAsync(
            CreateProduct(
                SKU_XXXV,
                "webcam XXXXX",
                323.00m,
                "Very good XXXXX"));

        await repository.AddAsync(
            CreateProduct(
                SKU_WWWYYY,
                "Mouse",
                49.90m,
                "Gaming mouse XXXXXX"));

        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var result = await repository.GetBySkuAsync(
            Sku.Create(SKU_WWWYYY));

        Assert.NotNull(result);
        Assert.Equal(SKU_WWWYYY, result.Sku.Value);
        Assert.Equal("Mouse", result.Name);
    }
}
