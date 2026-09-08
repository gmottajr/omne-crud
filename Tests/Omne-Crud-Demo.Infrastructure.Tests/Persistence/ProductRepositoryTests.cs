using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;
using Omne_Crud_Demo.Infrastructure.Persistence.DataRepositories;

namespace Omne_Crud_Demo.Infrastructure.Tests.Persistence;

public sealed class ProductRepositoryTests
{
    private const string SKU_XXXV = "XXXV-555123";

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, NullLogger<AppDbContext>.Instance);
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
}
