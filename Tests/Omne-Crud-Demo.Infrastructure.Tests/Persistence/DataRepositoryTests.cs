using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Omne_Crud_Demo.Domain;
using Omne_Crud_Demo.Infrastructure.Persistence.Data;
using Omne_Crud_Demo.Infrastructure.Persistence.DataRepositories;
using Omne_Crud_Demo.Infrastructure.Tests.Integration;

namespace Omne_Crud_Demo.Infrastructure.Tests.Persistence;

[Collection(InfrastructureIntegrationCollection.Name)]
public sealed class DataRepositoryTests
{
    private const string SKU_AGRYZ = "AGRYZ-73987";
    private const string SKU_JKLG = "JKLG-008456";
    private const string PROD_GIT = "guitar";
    private const string GIT_TYPE = "twelve-string";
    private const decimal PRICE = 1789.70m;

    private readonly InfrastructureDatabaseFixture _fixture;

    public DataRepositoryTests(
        InfrastructureDatabaseFixture fixture)
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

    private static DataRepository<Product, int> CreateRepository(
        AppDbContext context)
    {
        return new DataRepository<Product, int>(
            context,
            NullLogger<DataRepository<Product, int>>.Instance);
    }

    private static Product CreateProduct(
        string sku,
        string name = PROD_GIT,
        decimal price = PRICE,
        string description = GIT_TYPE)
    {
        return new Product(
            name,
            price,
            description,
            Sku.Create(sku));
    }

    [Fact]
    public async Task AddAsync_ShouldStageEntityAsAdded()
    {
        await using var context = CreateContext();

        var repository = CreateRepository(context);
        var product = CreateProduct(SKU_AGRYZ);

        await repository.AddAsync(product);

        var entry = context.Entry(product);

        Assert.Equal(EntityState.Added, entry.State);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistAddedEntity()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);
        var product = CreateProduct(SKU_AGRYZ);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var persistedProduct = await context.Products
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(SKU_AGRYZ, persistedProduct.Sku.Value);
        Assert.Equal(PROD_GIT, persistedProduct.Name);
        Assert.Equal(PRICE, persistedProduct.Price);
        Assert.Equal(GIT_TYPE, persistedProduct.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);
        var product = CreateProduct(SKU_AGRYZ);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var result = await repository.GetByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal(SKU_AGRYZ, result.Sku.Value);
        Assert.Equal(PROD_GIT, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var result = await repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        await ResetDatabaseAsync();

        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await repository.AddAsync(
            CreateProduct(
                SKU_AGRYZ,
                PROD_GIT,
                PRICE,
                GIT_TYPE));

        await repository.AddAsync(
            CreateProduct(
                SKU_JKLG,
                "Mouse",
                49.90m,
                "Gaming mouse"));

        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var result = await repository.GetAllAsync();

        Assert.Equal(2, result.Count);

        Assert.Contains(
            result,
            x => x.Sku.Value == SKU_AGRYZ);

        Assert.Contains(
            result,
            x => x.Sku.Value == SKU_JKLG);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyCollection_WhenNoEntitiesExist()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var result = await repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }


    [Fact]
    public async Task QueryAsync_ShouldReturnOnlyEntitiesMatchingPredicate()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await repository.AddAsync(
            CreateProduct(
                "ABC-001",
                PROD_GIT,
                PRICE,
                GIT_TYPE));

        await repository.AddAsync(
            CreateProduct(
                "ABC-002",
                "Mouse",
                49.90m,
                "Gaming mouse"));

        await repository.AddAsync(
            CreateProduct(
                "ABC-003",
                "Monitor",
                599.90m,
                "Gaming monitor"));

        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var result = await repository.QueryAsync(
            product => product.Price >= 500m && product.Price < 1000);

        var product = Assert.Single(result);

        Assert.Equal("Monitor", product.Name);
        Assert.Equal(599.90m, product.Price);
    }

    [Fact]
    public async Task QueryAsync_ShouldReturnEmptyCollection_WhenNoEntitiesMatchPredicate()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await repository.AddAsync(
            CreateProduct(SKU_AGRYZ));

        await repository.SaveChangesAsync();

        var result = await repository.QueryAsync(
            product => product.Price > 2000m);

        Assert.Empty(result);
    }

    [Fact]
    public async Task QueryAsync_ShouldApplyOrdering_WhenOrderByIsProvided()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await repository.AddAsync(
            CreateProduct(
                "ABC-001",
                PROD_GIT,
                PRICE,
                GIT_TYPE));

        await repository.AddAsync(
            CreateProduct(
                "ABC-002",
                "Mouse",
                49.90m,
                "Gaming mouse"));

        await repository.AddAsync(
            CreateProduct(
                "ABC-003",
                "Monitor",
                599.90m,
                "Gaming monitor"));

        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var result = await repository.QueryAsync(
            product => product.Price > 0,
            orderBy: query => query.OrderByDescending(x => x.Price));

        Assert.Collection(
            result,
            x => Assert.Equal(PROD_GIT, x.Name),
            x => Assert.Equal("Monitor", x.Name),
            x => Assert.Equal("Mouse", x.Name));
    }

    [Fact]
    public async Task QuerySingleAsync_ShouldReturnEntity_WhenExactlyOneEntityMatches()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await repository.AddAsync(
            CreateProduct(
                SKU_AGRYZ,
                PROD_GIT,
                PRICE,
                GIT_TYPE));

        await repository.AddAsync(
            CreateProduct(
                SKU_JKLG,
                "Mouse",
                49.90m,
                "Gaming mouse"));

        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var result = await repository.QuerySingleAsync(product => product.Sku == Sku.Create(SKU_JKLG));

        Assert.NotNull(result);
        Assert.Equal("Mouse", result.Name);
        Assert.Equal(SKU_JKLG, result.Sku.Value);
    }

    [Fact]
    public async Task QuerySingleAsync_ShouldReturnNull_WhenNoEntityMatches()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await repository.AddAsync(
            CreateProduct(SKU_AGRYZ));

        await repository.SaveChangesAsync();

        var result = await repository.QuerySingleAsync(
            product => product.Name == "Does not exist");

        Assert.Null(result);
    }

    [Fact]
    public async Task QuerySingleAsync_ShouldThrow_WhenMoreThanOneEntityMatches()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        await repository.AddAsync(
            CreateProduct(
                "ABC-001",
                PROD_GIT,
                PRICE,
                GIT_TYPE));

        await repository.AddAsync(
            CreateProduct(
                "ABC-002",
                "Mouse",
                PRICE,
                "Gaming mouse"));

        await repository.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.QuerySingleAsync(
                product => product.Price == PRICE));
    }

    [Fact]
    public async Task UpdateAsync_ShouldStageEntityAsModified()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var product = CreateProduct(SKU_AGRYZ);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var persistedProduct = await repository.GetByIdAsync(product.Id);

        Assert.NotNull(persistedProduct);

        persistedProduct.Update(
            "Gaming Keyboard",
            129.90m,
            "Mechanical gaming keyboard");

        await repository.UpdateAsync(persistedProduct);

        Assert.Equal(
            EntityState.Modified,
            context.Entry(persistedProduct).State);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistUpdatedValues_AfterSaveChanges()
    {
        await ResetDatabaseAsync();
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var product = CreateProduct(SKU_AGRYZ);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        product.Update(
            "Gaming Keyboard",
            129.90m,
            "Mechanical gaming keyboard");

        await repository.UpdateAsync(product);
        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var persistedProduct = await repository.GetByIdAsync(product.Id);

        Assert.NotNull(persistedProduct);
        Assert.Equal("Gaming Keyboard", persistedProduct.Name);
        Assert.Equal(129.90m, persistedProduct.Price);
        Assert.Equal(
            "Mechanical gaming keyboard",
            persistedProduct.Description);

        Assert.NotNull(persistedProduct.UpdatedAt);
    }

    
    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity_AfterSaveChanges()
    {
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var product = CreateProduct(SKU_AGRYZ);

        await repository.AddAsync(product);
        await repository.SaveChangesAsync();

        var deleted = await repository.DeleteAsync(product.Id);

        Assert.True(deleted);

        context.ChangeTracker.Clear();

        var result = await repository.GetByIdAsync(product.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_WhenEntityDoesNotExist()
    {
        await using var context = CreateContext();

        var repository = CreateRepository(context);

        var deleted = await repository.DeleteAsync(999);

        Assert.Equal(false, deleted);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();

        await context.Products.ExecuteDeleteAsync();
    }
}
