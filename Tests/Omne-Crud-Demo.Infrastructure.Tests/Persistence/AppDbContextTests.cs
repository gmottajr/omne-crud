using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;

namespace Omne_Crud_Demo.Infrastructure.Tests.Persistence;

public sealed class AppDbContextTests
{
    private const string SKU_ABYC = "ABYC-349123";

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(
            options,
            NullLogger<AppDbContext>.Instance);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSetCreatedAt_WhenEntityIsAdded()
    {
        await using var context = CreateContext();

        var product = BuildKeyboardProd();

        context.Products.Add(product);

        Assert.Equal(default, product.CreatedAt);

        await context.SaveChangesAsync();

        Assert.NotEqual(default, product.CreatedAt);
        Assert.Null(product.UpdatedAt);
    }

    
    [Fact]
    public async Task SaveChangesAsync_ShouldSetUpdatedAt_WhenEntityIsModified()
    {
        await using var context = CreateContext();

        var product = BuildKeyboardProd();

        context.Products.Add(product);
        await context.SaveChangesAsync();

        Assert.Null(product.UpdatedAt);

        product.Update(
            "Gaming Keyboard",
            129.90m,
            "Mechanical gaming keyboard");

        await context.SaveChangesAsync();

        Assert.NotNull(product.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotChangeCreatedAt_WhenEntityIsModified()
    {
        await using var context = CreateContext();

        var product = BuildKeyboardProd();

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createdAt = product.CreatedAt;

        product.Update(
            "Gaming Keyboard",
            129.90m,
            "Mechanical gaming keyboard");

        await context.SaveChangesAsync();

        Assert.Equal(createdAt, product.CreatedAt);
        Assert.NotNull(product.UpdatedAt);
    }

    
    [Fact]
    public async Task SaveChangesAsync_ShouldNotSetUpdatedAt_WhenEntityHasNotChanged()
    {
        await using var context = CreateContext();

        var product = BuildKeyboardProd();

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createdAt = product.CreatedAt;

        var affectedRows = await context.SaveChangesAsync();

        Assert.Equal(0, affectedRows);
        Assert.Equal(createdAt, product.CreatedAt);
        Assert.Null(product.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSetUpdatedAtAfterCreatedAt_WhenEntityIsModified()
    {
        await using var context = CreateContext();

        var product = BuildKeyboardProd();

        context.Products.Add(product);
        await context.SaveChangesAsync();

        product.Update(
            "Gaming Keyboard",
            129.90m,
            "Mechanical gaming keyboard");

        await context.SaveChangesAsync();

        Assert.NotNull(product.UpdatedAt);
        Assert.True(product.UpdatedAt >= product.CreatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSetCreatedAt_ForAllAddedEntities()
    {
        await using var context = CreateContext();

        var firstProduct = new Product(
            "Keyboard",
            99.90m,
            "Mechanical keyboard",
            Sku.Create(SKU_ABYC));

        var secondProduct = new Product(
            "Mouse",
            49.90m,
            "Gaming mouse",
            Sku.Create("ABC-456"));

        context.Products.AddRange(firstProduct, secondProduct);

        var affectedRows = await context.SaveChangesAsync();

        Assert.Equal(2, affectedRows);

        Assert.NotEqual(default, firstProduct.CreatedAt);
        Assert.NotEqual(default, secondProduct.CreatedAt);

        Assert.Null(firstProduct.UpdatedAt);
        Assert.Null(secondProduct.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldUseSameUpdatedAt_ForEntitiesModifiedInSameUnitOfWork()
    {
        await using var context = CreateContext();

        var firstProduct = BuildKeyboardProd();

        var secondProduct = new Product(
            "Mouse",
            49.90m,
            "Gaming mouse",
            Sku.Create("ABC-456"));

        context.Products.AddRange(firstProduct, secondProduct);
        await context.SaveChangesAsync();

        firstProduct.Update(
            "Gaming Keyboard",
            129.90m,
            "Mechanical gaming keyboard");

        secondProduct.Update(
            "Gaming Mouse",
            69.90m,
            "High precision gaming mouse");

        await context.SaveChangesAsync();

        Assert.NotNull(firstProduct.UpdatedAt);
        Assert.NotNull(secondProduct.UpdatedAt);

        Assert.Equal(
            firstProduct.UpdatedAt,
            secondProduct.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotModifyAuditFields_WhenEntityIsDeleted()
    {
        await using var context = CreateContext();

        var product = BuildKeyboardProd();

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var createdAt = product.CreatedAt;
        var updatedAt = product.UpdatedAt;

        context.Products.Remove(product);

        await context.SaveChangesAsync();

        Assert.Equal(createdAt, product.CreatedAt);
        Assert.Equal(updatedAt, product.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistProduct_WhenEntityIsAdded()
    {
        await using var context = CreateContext();

        var product = BuildKeyboardProd();

        context.Products.Add(product);

        var affectedRows = await context.SaveChangesAsync();

        Assert.Equal(1, affectedRows);

        context.ChangeTracker.Clear();

        var persistedProduct = await context.Products
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(SKU_ABYC, persistedProduct.Sku.Value);
        Assert.Equal("Keyboard", persistedProduct.Name);
        Assert.Equal(99.90m, persistedProduct.Price);
        Assert.Equal("Mechanical keyboard", persistedProduct.Description);

        Assert.NotEqual(default, persistedProduct.CreatedAt);
        Assert.Null(persistedProduct.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges_WhenEntityIsModified()
    {
        await using var context = CreateContext();

        var product = BuildKeyboardProd();

        context.Products.Add(product);
        await context.SaveChangesAsync();

        product.Update(
            "Gaming Keyboard",
            129.90m,
            "Mechanical gaming keyboard");

        var affectedRows = await context.SaveChangesAsync();

        Assert.Equal(1, affectedRows);

        context.ChangeTracker.Clear();

        var persistedProduct = await context.Products
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal("Gaming Keyboard", persistedProduct.Name);
        Assert.Equal(129.90m, persistedProduct.Price);
        Assert.Equal(
            "Mechanical gaming keyboard",
            persistedProduct.Description);

        Assert.NotNull(persistedProduct.UpdatedAt);
        Assert.True(persistedProduct.UpdatedAt >= persistedProduct.CreatedAt);
    }

    private static Product BuildKeyboardProd()
    {
        return new Product(
                    "Keyboard",
                    99.90m,
                    "Mechanical keyboard",
                    Sku.Create(SKU_ABYC));
    }

}
